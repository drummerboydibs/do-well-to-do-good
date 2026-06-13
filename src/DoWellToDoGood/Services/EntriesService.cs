using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DoWellToDoGood.Services;

/// <summary>
/// PostgREST data access. Every request carries the user's JWT and the
/// database enforces Row-Level Security, so users can only ever touch their
/// own rows. Payloads are opaque ciphertext by the time they get here.
/// </summary>
public class EntriesService(AuthService auth)
{
    private readonly HttpClient _http = new();

    public record KeyRecord(
        [property: JsonPropertyName("kek_salt")] string KekSalt,
        [property: JsonPropertyName("wrapped_dek")] string WrappedDek,
        [property: JsonPropertyName("recovery_salt")] string RecoverySalt,
        [property: JsonPropertyName("recovery_wrapped_dek")] string RecoveryWrappedDek);

    public record EntryRow(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("payload")] string Payload,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    public async Task<KeyRecord?> GetKeysAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            "user_keys?select=kek_salt,wrapped_dek,recovery_salt,recovery_wrapped_dek"));
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<KeyRecord>>();
        return rows is { Count: > 0 } ? rows[0] : null;
    }

    public async Task SaveKeysAsync(KeyRecord keys)
    {
        var req = Req(HttpMethod.Post, "user_keys");
        req.Content = JsonContent.Create(keys);
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    public async Task InsertEntryAsync(string payload)
    {
        var req = Req(HttpMethod.Post, "journal_entries");
        req.Content = JsonContent.Create(new Dictionary<string, string> { ["payload"] = payload });
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    public record EntryPage(IReadOnlyList<EntryRow> Rows, int Total);

    /// <summary>
    /// One page of entries (newest first) plus the total number of entries the
    /// user has, so the UI can show pagination controls and decrypt only the
    /// rows on the current page. The total comes from PostgREST's Content-Range
    /// header, which it includes when asked with "Prefer: count=exact".
    /// </summary>
    /// <remarks>
    /// There is intentionally no bulk "fetch all" method. The server only ever
    /// holds ciphertext (zero-knowledge encryption), so full-text search cannot
    /// run server-side, and fetching + decrypting every entry per query is the
    /// scaling problem this pagination exists to avoid. If search is added later,
    /// prefer a client-side session index (decrypt once) or blind indexing —
    /// storing keyed HMAC token hashes beside the ciphertext so the server can
    /// match tokens without seeing plaintext — rather than reintroducing fetch-all.
    /// </remarks>
    public async Task<EntryPage> ListEntriesPageAsync(int offset, int limit)
    {
        var req = Req(HttpMethod.Get,
            $"journal_entries?select=id,payload,created_at&order=created_at.desc&offset={offset}&limit={limit}");
        req.Headers.Add("Prefer", "count=exact");
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<EntryRow>>() ?? new();
        // Content-Range is normally a content header, but be tolerant of it
        // arriving on the response headers; fall back to the page size if absent.
        var contentRange =
            (res.Content.Headers.TryGetValues("Content-Range", out var cv) ? cv.FirstOrDefault() : null)
            ?? (res.Headers.TryGetValues("Content-Range", out var rv) ? rv.FirstOrDefault() : null);
        var total = ParseContentRangeTotal(contentRange) ?? rows.Count;
        return new EntryPage(rows, total);
    }

    /// <summary>
    /// Pull the total row count out of a PostgREST Content-Range header, which
    /// looks like "0-9/57" (or "*/0" when empty) — we want the part after the
    /// slash. Returns null if it's missing, "*", or otherwise unparseable.
    /// </summary>
    internal static int? ParseContentRangeTotal(string? contentRange)
    {
        if (string.IsNullOrWhiteSpace(contentRange)) return null;
        var slash = contentRange.IndexOf('/');
        if (slash < 0) return null;
        var totalPart = contentRange[(slash + 1)..].Trim();
        return int.TryParse(totalPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var total)
            ? total
            : null;
    }

    public record TimestampRow([property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    /// <summary>
    /// Just the creation timestamps — enough to count saved entries and build a
    /// streak without pulling (or decrypting) any ciphertext payloads.
    /// </summary>
    public async Task<List<DateTimeOffset>> ListEntryTimestampsAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            "journal_entries?select=created_at&order=created_at.desc"));
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<TimestampRow>>() ?? new();
        return rows.Select(r => r.CreatedAt).ToList();
    }

    public async Task DeleteEntryAsync(Guid id)
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Delete, $"journal_entries?id=eq.{id}"));
        res.EnsureSuccessStatusCode();
    }

    private HttpRequestMessage Req(HttpMethod method, string pathAndQuery)
    {
        var req = new HttpRequestMessage(method, $"{SupabaseConfig.Url}/rest/v1/{pathAndQuery}");
        req.Headers.Add("apikey", SupabaseConfig.PublishableKey);
        req.Headers.Add("Authorization", $"Bearer {auth.AccessToken}");
        return req;
    }
}
