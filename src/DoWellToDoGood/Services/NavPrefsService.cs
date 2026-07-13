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
    internal sealed record Wire(
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
        switch (DecideSync(_updatedAt, server?.UpdatedAt))
        {
            case SyncAction.AdoptServer:
                Adopt(server!);          // a newer layout from another device wins
                await SaveLocalAsync();
                Changed?.Invoke();
                break;
            case SyncAction.PushLocal:
                await PushServerAsync(); // this device is ahead (or server empty) — push it up
                break;
        }
    }

    internal enum SyncAction { None, AdoptServer, PushLocal }

    /// <summary>
    /// Last-write-wins decision on unlock. A newer server copy is adopted; a
    /// newer local copy (or the very first customization when the server has
    /// none) is pushed up; identical timestamps do nothing.
    /// </summary>
    internal static SyncAction DecideSync(DateTimeOffset local, DateTimeOffset? server)
    {
        if (server is null) return local > DateTimeOffset.MinValue ? SyncAction.PushLocal : SyncAction.None;
        if (server.Value > local) return SyncAction.AdoptServer;
        if (local > server.Value) return SyncAction.PushLocal;
        return SyncAction.None;
    }

    // ---- Reads ----------------------------------------------------------

    public bool IsHidden(string key) => _hidden.Contains(key);

    /// <summary>Every reorderable item in user order — for the Account editor.</summary>
    public IReadOnlyList<NavItem> EditorItems() =>
        _order.Select(NavCatalog.ById).OfType<NavItem>().ToList();

    /// <summary>Visible items for the current auth state, in user order.</summary>
    public IReadOnlyList<NavItem> VisibleItems(bool signedIn) => ComputeVisible(_order, _hidden, signedIn);

    /// <summary>Pure projection of an order + hidden set to the visible items for an auth state.</summary>
    internal static IReadOnlyList<NavItem> ComputeVisible(IEnumerable<string> order, ISet<string> hidden, bool signedIn) =>
        order.Select(NavCatalog.ById)
             .OfType<NavItem>()
             .Where(i => !hidden.Contains(i.Key) && (signedIn || !i.RequiresAuth))
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
        var moved = ApplyMove(_order, key, delta);
        if (moved.SequenceEqual(_order)) return Task.CompletedTask; // unknown key or clamped no-op
        _order = moved;
        return CommitAsync();
    }

    /// <summary>Pure reorder: returns a new list with <paramref name="key"/> shifted by
    /// <paramref name="delta"/>, clamped to the ends. Unknown keys return the list unchanged.</summary>
    internal static List<string> ApplyMove(IReadOnlyList<string> order, string key, int delta)
    {
        var result = order.ToList();
        var i = result.IndexOf(key);
        if (i < 0) return result;
        var target = Math.Clamp(i + delta, 0, result.Count - 1);
        if (target == i) return result;
        result.RemoveAt(i);
        result.Insert(target, key);
        return result;
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
        _order = ReconcileOrder(savedOrder);
        _hidden = ReconcileHidden(savedHidden);
    }

    /// <summary>Saved order, filtered to known keys (deduped) with any newly-added catalog pages appended.</summary>
    internal static List<string> ReconcileOrder(IEnumerable<string>? savedOrder)
    {
        var valid = NavCatalog.DefaultOrder();
        var order = (savedOrder ?? Enumerable.Empty<string>())
            .Where(valid.Contains).Distinct().ToList();
        foreach (var key in valid.Where(k => !order.Contains(k)))
            order.Add(key);
        return order;
    }

    /// <summary>Saved hidden set, filtered to known reorderable keys.</summary>
    internal static HashSet<string> ReconcileHidden(IEnumerable<string>? savedHidden) =>
        (savedHidden ?? Enumerable.Empty<string>()).Where(NavCatalog.DefaultOrder().Contains).ToHashSet();

    private Wire Snapshot() => new(_order.ToList(), _hidden.ToList(), _updatedAt);

    internal static string Serialize(Wire wire) => JsonSerializer.Serialize(wire);

    internal static Wire? Deserialize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try { return JsonSerializer.Deserialize<Wire>(raw); }
        catch { return null; }
    }

    private async Task SaveLocalAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, Serialize(Snapshot()));
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
