using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DoWellToDoGood.Services;

/// <summary>
/// PostgREST data access for recovery ("days sober") counters. Same model as
/// <see cref="TherapyService"/>: every request carries the user's JWT, the
/// database enforces Row-Level Security, and <c>payload</c> is opaque ciphertext
/// by the time it reaches here. Unlike the therapy tables, there are no
/// structural columns at all beyond the row id and timestamp — the clean-since
/// date, streak lengths, and label all live inside the encrypted payload, so a
/// lone row can't even reveal that someone recently had a setback.
/// </summary>
public class SobrietyService(AuthService auth)
{
    private readonly HttpClient _http = new();

    public record CounterRow(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("payload")] string Payload,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    private record IdRow([property: JsonPropertyName("id")] Guid Id);

    /// <summary>All of the user's counters, oldest first (stable display order).</summary>
    public async Task<List<CounterRow>> ListCountersAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            "sobriety_counters?select=id,payload,created_at&order=created_at.asc"));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<CounterRow>>() ?? new();
    }

    public Task<Guid> InsertCounterAsync(string payload) =>
        InsertReturningIdAsync("sobriety_counters", new() { ["payload"] = payload });

    public Task UpdateCounterAsync(Guid id, string payload) =>
        PatchAsync($"sobriety_counters?id=eq.{id}", new() { ["payload"] = payload });

    public Task DeleteCounterAsync(Guid id) => DeleteAsync($"sobriety_counters?id=eq.{id}");

    // ---------- Helpers (identical shape to TherapyService) ----------

    private async Task<Guid> InsertReturningIdAsync(string table, Dictionary<string, object?> body)
    {
        var req = Req(HttpMethod.Post, $"{table}?select=id");
        req.Headers.Add("Prefer", "return=representation");
        req.Content = JsonContent.Create(body);
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<IdRow>>();
        return rows is { Count: > 0 }
            ? rows[0].Id
            : throw new InvalidOperationException("Insert did not return an id.");
    }

    private async Task PatchAsync(string pathAndQuery, Dictionary<string, object?> body)
    {
        var req = Req(HttpMethod.Patch, pathAndQuery);
        req.Content = JsonContent.Create(body);
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    private async Task DeleteAsync(string pathAndQuery)
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Delete, pathAndQuery));
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
