using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DoWellToDoGood.Services;

/// <summary>
/// PostgREST data access for the sleep journal. Same privacy model as the rest
/// of the app: every request carries the user's JWT, the database enforces
/// Row-Level Security, and <c>payload</c> is opaque ciphertext by the time it
/// arrives here. The only structural column is <c>night_date</c> (one row per
/// night), which lets us fetch a date range and replace a night without ever
/// decrypting server-side.
/// </summary>
public class SleepService(AuthService auth)
{
    private readonly HttpClient _http = new();

    public record NightRow(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("night_date")] DateOnly NightDate,
        [property: JsonPropertyName("payload")] string Payload);

    /// <summary>Rows whose night falls within [from, to], newest night first.</summary>
    public async Task<List<NightRow>> ListRangeAsync(DateOnly from, DateOnly to)
    {
        var f = from.ToString("yyyy-MM-dd");
        var t = to.ToString("yyyy-MM-dd");
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            $"sleep_entries?select=id,night_date,payload&night_date=gte.{f}&night_date=lte.{t}&order=night_date.desc"));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<NightRow>>() ?? new();
    }

    /// <summary>The single row for a given night, or null if none is logged yet.</summary>
    public async Task<NightRow?> GetByNightAsync(DateOnly night)
    {
        var d = night.ToString("yyyy-MM-dd");
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            $"sleep_entries?select=id,night_date,payload&night_date=eq.{d}&limit=1"));
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<NightRow>>();
        return rows is { Count: > 0 } ? rows[0] : null;
    }

    /// <summary>
    /// Insert or replace the row for a night. Upserts on the (user_id,
    /// night_date) unique key so re-saving the same morning overwrites cleanly.
    /// </summary>
    public async Task SaveNightAsync(DateOnly night, string payload)
    {
        var req = Req(HttpMethod.Post, "sleep_entries?on_conflict=user_id,night_date");
        req.Headers.Add("Prefer", "resolution=merge-duplicates");
        req.Content = JsonContent.Create(new Dictionary<string, object?>
        {
            ["night_date"] = night.ToString("yyyy-MM-dd"),
            ["payload"] = payload,
        });
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteNightAsync(Guid id)
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Delete, $"sleep_entries?id=eq.{id}"));
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
