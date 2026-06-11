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

    public async Task<List<EntryRow>> ListEntriesAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            "journal_entries?select=id,payload,created_at&order=created_at.desc"));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<EntryRow>>() ?? new();
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
