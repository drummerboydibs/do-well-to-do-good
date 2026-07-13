using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Services;

/// <summary>
/// The user's navigation layout: which reorderable pages show, and in what
/// order (Home and Account are structural anchors, handled outside this).
///
/// Storage is hybrid, because the nav is global chrome shown on every page,
/// yet the vault re-locks on every load:
///  • a plaintext copy in localStorage applies instantly on each load (and is
///    the only store for guests);
///  • an envelope-encrypted copy in <c>user_prefs.nav_payload</c> syncs across
///    devices — a DBA sees only ciphertext, so tab order never leaks server-side.
/// On unlock the two are reconciled last-write-wins via an embedded timestamp.
/// Mirrors the reactive shape of <see cref="ThemeService"/> / <see cref="FaithService"/>.
/// </summary>
public class NavPrefsService
{
    private const string StorageKey = "dwtdg.nav";

    /// <summary>How many reorderable tabs the bottom bar shows before "More".</summary>
    public const int BottomBarSlots = 3;

    private readonly IJSRuntime _js;
    private readonly AuthService _auth;
    private readonly CryptoService _crypto;
    private readonly HttpClient _http = new();

    private List<string> _order = NavCatalog.DefaultOrder();
    private HashSet<string> _hidden = new();
    private DateTimeOffset _updatedAt = DateTimeOffset.MinValue; // MinValue = never customised

    public event Action? Changed;

    public NavPrefsService(IJSRuntime js, AuthService auth, CryptoService crypto)
    {
        _js = js;
        _auth = auth;
        _crypto = crypto;
        _crypto.Changed += OnCryptoChanged;
    }

    // Serialised form for both the local cache and the encrypted server payload.
    private sealed record Wire(
        [property: JsonPropertyName("order")] List<string> Order,
        [property: JsonPropertyName("hidden")] List<string> Hidden,
        [property: JsonPropertyName("updatedAt")] DateTimeOffset UpdatedAt);

    // ---- Lifecycle ------------------------------------------------------

    /// <summary>Apply the local cache immediately (called once at startup).</summary>
    public async Task InitializeAsync()
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            var wire = Deserialize(raw);
            if (wire is not null) Adopt(wire);
        }
        catch { /* no storage / bad data — keep defaults */ }
    }

    // When the vault unlocks we can finally read/write the encrypted server copy.
    // Locking is a no-op here: the local cache must keep driving the visible nav.
    private async void OnCryptoChanged()
    {
        if (!_crypto.IsUnlocked) return;
        try { await SyncOnUnlockAsync(); }
        catch { /* offline / server hiccup — local cache still drives the UI */ }
    }

    private async Task SyncOnUnlockAsync()
    {
        if (!_auth.IsSignedIn || !_crypto.IsUnlocked) return;

        var server = await FetchServerAsync();
        if (server is not null)
        {
            if (server.UpdatedAt > _updatedAt)
            {
                Adopt(server);          // a newer layout from another device wins
                await SaveLocalAsync();
                Changed?.Invoke();
            }
            else if (_updatedAt > server.UpdatedAt)
            {
                await PushServerAsync(); // this device is ahead — push it up
            }
        }
        else if (_updatedAt > DateTimeOffset.MinValue)
        {
            await PushServerAsync();     // nothing server-side yet, but we have local edits
        }
    }

    // ---- Reads ----------------------------------------------------------

    public bool IsHidden(string key) => _hidden.Contains(key);

    /// <summary>Every reorderable item in user order — for the Account editor.</summary>
    public IReadOnlyList<NavItem> EditorItems() =>
        _order.Select(NavCatalog.ById).OfType<NavItem>().ToList();

    /// <summary>Visible items for the current auth state, in user order.</summary>
    public IReadOnlyList<NavItem> VisibleItems(bool signedIn) =>
        _order.Select(NavCatalog.ById)
              .OfType<NavItem>()
              .Where(i => !_hidden.Contains(i.Key) && (signedIn || !i.RequiresAuth))
              .ToList();

    // ---- Writes ---------------------------------------------------------

    public Task ToggleHiddenAsync(string key, bool hidden)
    {
        if (NavCatalog.ById(key) is not { Fixed: false }) return Task.CompletedTask;
        if (hidden) _hidden.Add(key); else _hidden.Remove(key);
        return CommitAsync();
    }

    /// <summary>Move an item earlier (delta &lt; 0) or later (delta &gt; 0) in the order.</summary>
    public Task MoveAsync(string key, int delta)
    {
        var i = _order.IndexOf(key);
        if (i < 0) return Task.CompletedTask;
        var target = Math.Clamp(i + delta, 0, _order.Count - 1);
        if (target == i) return Task.CompletedTask;
        _order.RemoveAt(i);
        _order.Insert(target, key);
        return CommitAsync();
    }

    public Task ResetAsync()
    {
        _order = NavCatalog.DefaultOrder();
        _hidden.Clear();
        return CommitAsync();
    }

    // Every edit stamps the time, saves locally (instant + cross-session on this
    // device), and — when we can — pushes the encrypted copy for other devices.
    private async Task CommitAsync()
    {
        _updatedAt = DateTimeOffset.UtcNow;
        await SaveLocalAsync();
        if (_auth.IsSignedIn && _crypto.IsUnlocked)
        {
            try { await PushServerAsync(); }
            catch { /* will re-sync on next unlock (local copy is newer) */ }
        }
        Changed?.Invoke();
    }

    // ---- Persistence helpers -------------------------------------------

    private void Adopt(Wire wire)
    {
        Reconcile(wire.Order, wire.Hidden);
        _updatedAt = wire.UpdatedAt;
    }

    /// <summary>
    /// Merge a saved layout with the current catalog so the app survives pages
    /// being added or removed between versions: drop unknown keys, append new ones.
    /// </summary>
    private void Reconcile(IEnumerable<string>? savedOrder, IEnumerable<string>? savedHidden)
    {
        var valid = NavCatalog.DefaultOrder();
        var order = (savedOrder ?? Enumerable.Empty<string>())
            .Where(valid.Contains).Distinct().ToList();
        foreach (var key in valid.Where(k => !order.Contains(k)))
            order.Add(key);

        _order = order;
        _hidden = (savedHidden ?? Enumerable.Empty<string>()).Where(valid.Contains).ToHashSet();
    }

    private Wire Snapshot() => new(_order.ToList(), _hidden.ToList(), _updatedAt);

    private static Wire? Deserialize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try { return JsonSerializer.Deserialize<Wire>(raw); }
        catch { return null; }
    }

    private async Task SaveLocalAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Snapshot());
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch { /* private mode — in-memory order still drives this session */ }
    }

    // ---- PostgREST (user_prefs.nav_payload, one row per user; writes upsert) ----

    private async Task<Wire?> FetchServerAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get, "user_prefs?select=nav_payload&limit=1"));
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<NavPayloadRow>>();
        var payload = rows is { Count: > 0 } ? rows[0].NavPayload : null;
        if (string.IsNullOrEmpty(payload)) return null;
        return Deserialize(await _crypto.DecryptAsync(payload));
    }

    private async Task PushServerAsync()
    {
        var cipher = await _crypto.EncryptAsync(JsonSerializer.Serialize(Snapshot()));
        var req = Req(HttpMethod.Post, "user_prefs?on_conflict=user_id");
        req.Headers.Add("Prefer", "resolution=merge-duplicates");
        req.Content = JsonContent.Create(new Dictionary<string, object?>
        {
            ["nav_payload"] = cipher,
            ["updated_at"] = DateTimeOffset.UtcNow
        });
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    private record NavPayloadRow([property: JsonPropertyName("nav_payload")] string? NavPayload);

    private HttpRequestMessage Req(HttpMethod method, string pathAndQuery)
    {
        var req = new HttpRequestMessage(method, $"{SupabaseConfig.Url}/rest/v1/{pathAndQuery}");
        req.Headers.Add("apikey", SupabaseConfig.PublishableKey);
        req.Headers.Add("Authorization", $"Bearer {_auth.AccessToken}");
        return req;
    }
}
