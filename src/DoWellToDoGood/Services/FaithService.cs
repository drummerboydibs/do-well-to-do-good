using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Services;

/// <summary>
/// The user's selected faith/belief traditions (issue #29). Because religion is
/// sensitive, the selection is stored the same zero-knowledge way as journal
/// entries — encrypted client-side into the single <c>user_prefs</c> row — so it
/// only exists in the clear while the vault is unlocked. The current selection is
/// cached in memory and reloaded whenever the vault unlocks; components listen to
/// <see cref="Changed"/>. Mirrors the reactive shape of <see cref="ThemeService"/>.
/// </summary>
public class FaithService
{
    private readonly AuthService _auth;
    private readonly CryptoService _crypto;
    private readonly HttpClient _http = new();

    private List<FaithTradition> _selected = new();
    private bool _loaded;

    public FaithService(AuthService auth, CryptoService crypto)
    {
        _auth = auth;
        _crypto = crypto;
        _crypto.Changed += OnCryptoChanged;
    }

    /// <summary>Traditions the user has opted into (empty while locked or none chosen).</summary>
    public IReadOnlyList<FaithTradition> Selected => _selected;

    public event Action? Changed;

    private async void OnCryptoChanged()
    {
        if (_crypto.IsUnlocked)
        {
            await LoadAsync(force: true);
        }
        else
        {
            _selected = new();   // locking wipes the plaintext selection from memory
            _loaded = false;
        }
        Changed?.Invoke();
    }

    /// <summary>Load the selection once the vault is unlocked; a no-op otherwise.</summary>
    public async Task LoadAsync(bool force = false)
    {
        if (_loaded && !force) return;
        if (!_auth.IsSignedIn || !_crypto.IsUnlocked) { _selected = new(); return; }
        try
        {
            var list = new List<FaithTradition>();
            var payload = await GetPayloadAsync();
            if (!string.IsNullOrEmpty(payload))
            {
                using var doc = JsonDocument.Parse(await _crypto.DecryptAsync(payload));
                if (doc.RootElement.TryGetProperty("traditions", out var arr) && arr.ValueKind == JsonValueKind.Array)
                    foreach (var el in arr.EnumerateArray())
                        if (Enum.TryParse<FaithTradition>(el.GetString(), out var t) && !list.Contains(t))
                            list.Add(t);
            }
            _selected = list;
            _loaded = true;
        }
        catch { _selected = new(); }
    }

    /// <summary>Persist a new selection (encrypted) and notify listeners.</summary>
    public async Task SetAsync(IEnumerable<FaithTradition> traditions)
    {
        _selected = traditions.Distinct().ToList();
        var json = JsonSerializer.Serialize(new { traditions = _selected.Select(t => t.ToString()).ToArray() });
        var payload = await _crypto.EncryptAsync(json);
        await UpsertPayloadAsync(payload);
        _loaded = true;
        Changed?.Invoke();
    }

    // ---------- PostgREST (user_prefs, one row per user; writes upsert) ----------

    private async Task<string?> GetPayloadAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get, "user_prefs?select=payload&limit=1"));
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<PayloadRow>>();
        return rows is { Count: > 0 } ? rows[0].Payload : null;
    }

    private async Task UpsertPayloadAsync(string payload)
    {
        var req = Req(HttpMethod.Post, "user_prefs?on_conflict=user_id");
        req.Headers.Add("Prefer", "resolution=merge-duplicates");
        req.Content = JsonContent.Create(new Dictionary<string, object?>
        {
            ["payload"] = payload,
            ["updated_at"] = DateTimeOffset.UtcNow
        });
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    private record PayloadRow([property: JsonPropertyName("payload")] string Payload);

    private HttpRequestMessage Req(HttpMethod method, string pathAndQuery)
    {
        var req = new HttpRequestMessage(method, $"{SupabaseConfig.Url}/rest/v1/{pathAndQuery}");
        req.Headers.Add("apikey", SupabaseConfig.PublishableKey);
        req.Headers.Add("Authorization", $"Bearer {_auth.AccessToken}");
        return req;
    }
}
