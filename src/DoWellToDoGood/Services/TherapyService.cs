using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DoWellToDoGood.Services;

/// <summary>
/// PostgREST data access for therapy sessions, goals, and goal progress.
/// Same model as <see cref="EntriesService"/>: every request carries the user's
/// JWT, the database enforces Row-Level Security, and all payloads are opaque
/// ciphertext (encrypted client-side) by the time they reach here. Only
/// structural columns (the done flag, foreign keys, timestamps) are in the clear.
/// </summary>
public class TherapyService(AuthService auth)
{
    private readonly HttpClient _http = new();

    public record SessionRow(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("payload")] string Payload,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
        [property: JsonPropertyName("updated_at")] DateTimeOffset? UpdatedAt);

    public record SessionPage(IReadOnlyList<SessionRow> Rows, int Total);

    public record SessionOption(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    public record GoalRow(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("session_id")] Guid? SessionId,
        [property: JsonPropertyName("payload")] string Payload,
        [property: JsonPropertyName("done")] bool Done,
        [property: JsonPropertyName("end_date")] DateOnly? EndDate,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    public record ProgressRow(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("goal_id")] Guid GoalId,
        [property: JsonPropertyName("payload")] string Payload,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    private record IdRow([property: JsonPropertyName("id")] Guid Id);

    // ---------- Sessions ----------

    public Task<Guid> InsertSessionAsync(string payload) =>
        InsertReturningIdAsync("therapy_sessions", new() { ["payload"] = payload });

    /// <summary>
    /// One page of sessions (newest first) plus the total count, so the UI can
    /// paginate without decrypting every session. Ordered by <c>created_at</c> —
    /// the session's own date lives inside the encrypted payload, so the server
    /// can't sort by it (in practice a session is logged around when it happened,
    /// so the two orders line up).
    /// </summary>
    public async Task<SessionPage> ListSessionsPageAsync(int offset, int limit)
    {
        var req = Req(HttpMethod.Get,
            $"therapy_sessions?select=id,payload,created_at,updated_at&order=created_at.desc&offset={offset}&limit={limit}");
        req.Headers.Add("Prefer", "count=exact");
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        var rows = await res.Content.ReadFromJsonAsync<List<SessionRow>>() ?? new();
        return new SessionPage(rows, Pagination.TotalFromResponse(res, rows.Count));
    }

    /// <summary>
    /// Every session as just (id, created_at) — no payloads, so nothing has to be
    /// fetched in bulk or decrypted. Enough to populate a "link to a session"
    /// picker with the complete list regardless of which page the user is viewing.
    /// The label is necessarily the logged date rather than the session's own
    /// (encrypted) date; the two line up in practice, for the same reason session
    /// paging orders by created_at.
    /// </summary>
    public async Task<List<SessionOption>> ListSessionOptionsAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            "therapy_sessions?select=id,created_at&order=created_at.desc"));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<SessionOption>>() ?? new();
    }

    /// <summary>
    /// Replace a session's ciphertext and stamp <c>updated_at</c>; returns the
    /// timestamp written so the caller can show "edited on …" without a re-fetch.
    /// </summary>
    public async Task<DateTimeOffset> UpdateSessionAsync(Guid id, string payload)
    {
        var editedAt = DateTimeOffset.UtcNow;
        await PatchAsync($"therapy_sessions?id=eq.{id}", new()
        {
            ["payload"] = payload,
            ["updated_at"] = editedAt
        });
        return editedAt;
    }

    public Task DeleteSessionAsync(Guid id) => DeleteAsync($"therapy_sessions?id=eq.{id}");

    // ---------- Goals ----------

    public Task<Guid> InsertGoalAsync(Guid? sessionId, string payload, DateOnly? endDate) =>
        InsertReturningIdAsync("goals", new()
        {
            ["payload"] = payload,
            ["session_id"] = sessionId,
            ["end_date"] = endDate?.ToString("yyyy-MM-dd")
        });

    public async Task<List<GoalRow>> ListGoalsAsync(bool openOnly)
    {
        var filter = openOnly ? "&done=is.false" : "";
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            $"goals?select=id,session_id,payload,done,end_date,created_at{filter}&order=created_at.desc"));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<GoalRow>>() ?? new();
    }

    public Task UpdateGoalAsync(Guid id, string payload, DateOnly? endDate) =>
        PatchAsync($"goals?id=eq.{id}", new()
        {
            ["payload"] = payload,
            ["end_date"] = endDate?.ToString("yyyy-MM-dd")
        });

    public Task SetGoalDoneAsync(Guid id, bool done) =>
        PatchAsync($"goals?id=eq.{id}", new() { ["done"] = done });

    public Task DeleteGoalAsync(Guid id) => DeleteAsync($"goals?id=eq.{id}");

    // ---------- Progress ----------

    public async Task InsertProgressAsync(Guid goalId, string payload)
    {
        var req = Req(HttpMethod.Post, "goal_progress");
        req.Content = JsonContent.Create(new Dictionary<string, object?> { ["goal_id"] = goalId, ["payload"] = payload });
        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }

    /// <summary>All progress rows for the user (newest first), grouped client-side by goal.</summary>
    public async Task<List<ProgressRow>> ListAllProgressAsync()
    {
        using var res = await _http.SendAsync(Req(HttpMethod.Get,
            "goal_progress?select=id,goal_id,payload,created_at&order=created_at.desc"));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<ProgressRow>>() ?? new();
    }

    public Task DeleteProgressAsync(Guid id) => DeleteAsync($"goal_progress?id=eq.{id}");

    // ---------- Helpers ----------

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
