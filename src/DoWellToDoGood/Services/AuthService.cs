using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DoWellToDoGood.Services;

/// <summary>
/// Minimal Supabase (GoTrue) email magic-link client. No passwords exist
/// anywhere in this system. Tokens are held in memory and mirrored to
/// localStorage so a page refresh keeps you signed in; encryption keys are
/// handled separately (CryptoService) and are never persisted at all.
/// </summary>
public class AuthService(IJSRuntime js, NavigationManager nav)
{
    private const string StorageKey = "dwtdg.session";
    private readonly HttpClient _http = new();
    private long _expiresAt; // unix seconds

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? Email { get; private set; }
    public string? UserId { get; private set; }

    public bool IsSignedIn => AccessToken is not null;
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        // 1) Returning from a magic link? GoTrue puts tokens in the URL fragment.
        var fragment = new Uri(nav.Uri).Fragment.TrimStart('#');
        if (fragment.Contains("access_token="))
        {
            var p = ParseFragment(fragment);
            if (p.TryGetValue("access_token", out var at) && p.TryGetValue("refresh_token", out var rt))
            {
                ApplySession(at, rt, p.TryGetValue("expires_in", out var ei) && long.TryParse(ei, out var s) ? s : 3600);
                await PersistAsync();
                await js.InvokeVoidAsync("dwtdgUtil.clearHash"); // keep tokens out of history
                Changed?.Invoke();
                return;
            }
        }

        // 2) Otherwise restore a stored session, refreshing if it's stale.
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (json is null) return;
        try
        {
            var s = JsonSerializer.Deserialize<StoredSession>(json);
            if (s is null) return;
            AccessToken = s.AccessToken;
            RefreshToken = s.RefreshToken;
            _expiresAt = s.ExpiresAt;
            ReadClaims();
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > _expiresAt - 120 && !await RefreshAsync())
            {
                await SignOutAsync(localOnly: true);
                return;
            }
            Changed?.Invoke();
        }
        catch
        {
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }

    public async Task<(bool Ok, string? Error)> SendMagicLinkAsync(string email)
    {
        var redirect = Uri.EscapeDataString(nav.BaseUri);
        using var req = NewReq(HttpMethod.Post, $"{SupabaseConfig.Url}/auth/v1/otp?redirect_to={redirect}");
        req.Content = JsonContent.Create(new { email, create_user = true });
        using var res = await _http.SendAsync(req);
        if (res.IsSuccessStatusCode) return (true, null);
        var body = await res.Content.ReadAsStringAsync();
        return (false, ExtractError(body) ?? $"Request failed ({(int)res.StatusCode}).");
    }

    public async Task<bool> RefreshAsync()
    {
        if (RefreshToken is null) return false;
        using var req = NewReq(HttpMethod.Post, $"{SupabaseConfig.Url}/auth/v1/token?grant_type=refresh_token");
        req.Content = JsonContent.Create(new { refresh_token = RefreshToken });
        using var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode) return false;
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        ApplySession(
            root.GetProperty("access_token").GetString()!,
            root.GetProperty("refresh_token").GetString()!,
            root.TryGetProperty("expires_in", out var ei) ? ei.GetInt64() : 3600);
        await PersistAsync();
        return true;
    }

    public async Task SignOutAsync(bool localOnly = false)
    {
        if (!localOnly && AccessToken is not null)
        {
            try
            {
                using var req = NewReq(HttpMethod.Post, $"{SupabaseConfig.Url}/auth/v1/logout");
                req.Headers.Add("Authorization", $"Bearer {AccessToken}");
                await _http.SendAsync(req);
            }
            catch { /* best effort — local sign-out still proceeds */ }
        }
        AccessToken = RefreshToken = Email = UserId = null;
        _expiresAt = 0;
        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        Changed?.Invoke();
    }

    /// <summary>
    /// Fetches account timestamps (joined date, last sign-in) from GoTrue's
    /// /user endpoint for the Account page. Returns nulls on any failure so the
    /// UI simply omits the lines rather than erroring.
    /// </summary>
    public async Task<(DateTimeOffset? CreatedAt, DateTimeOffset? LastSignIn)> GetUserMetaAsync()
    {
        if (AccessToken is null) return (null, null);
        try
        {
            using var req = NewReq(HttpMethod.Get, $"{SupabaseConfig.Url}/auth/v1/user");
            req.Headers.Add("Authorization", $"Bearer {AccessToken}");
            using var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode) return (null, null);
            return ParseUserMeta(await res.Content.ReadAsStringAsync());
        }
        catch { return (null, null); }
    }

    /// <summary>
    /// Pull the "created_at" and "last_sign_in_at" timestamps out of a GoTrue
    /// /user response. Either is null if it's absent or unparseable; a malformed
    /// body yields (null, null) rather than throwing.
    /// </summary>
    internal static (DateTimeOffset? CreatedAt, DateTimeOffset? LastSignIn) ParseUserMeta(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return (ReadTimestamp(root, "created_at"), ReadTimestamp(root, "last_sign_in_at"));
        }
        catch { return (null, null); }

        static DateTimeOffset? ReadTimestamp(JsonElement root, string name) =>
            root.TryGetProperty(name, out var el) && el.GetString() is { } s
                && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var v)
                ? v
                : null;
    }

    private static HttpRequestMessage NewReq(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Add("apikey", SupabaseConfig.PublishableKey);
        return req;
    }

    private void ApplySession(string accessToken, string refreshToken, long expiresIn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        _expiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn;
        ReadClaims();
    }

    private void ReadClaims()
    {
        Email = UserId = null;
        if (AccessToken is null) return;
        (Email, UserId) = DecodeClaims(AccessToken);
    }

    /// <summary>
    /// Pull the display-only "email" and "sub" claims out of a JWT's payload
    /// (base64url, unverified — the server verifies the token itself). Returns
    /// nulls for anything malformed rather than throwing.
    /// </summary>
    internal static (string? Email, string? UserId) DecodeClaims(string accessToken)
    {
        try
        {
            var payload = accessToken.Split('.')[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var email = doc.RootElement.TryGetProperty("email", out var e) ? e.GetString() : null;
            var userId = doc.RootElement.TryGetProperty("sub", out var s) ? s.GetString() : null;
            return (email, userId);
        }
        catch { return (null, null); }
    }

    private async Task PersistAsync()
    {
        var json = JsonSerializer.Serialize(new StoredSession(AccessToken!, RefreshToken!, _expiresAt));
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    internal static Dictionary<string, string> ParseFragment(string fragment) =>
        fragment.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(kv => kv.Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));

    internal static string? ExtractError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            foreach (var key in new[] { "msg", "message", "error_description", "error" })
                if (doc.RootElement.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String)
                    return v.GetString();
        }
        catch { }
        return null;
    }

    private record StoredSession(string AccessToken, string RefreshToken, long ExpiresAt);
}
