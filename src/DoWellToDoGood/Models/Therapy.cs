using System.Text.Json;

namespace DoWellToDoGood.Models;

// Plaintext payloads for the therapy page's three tables. Each codec takes and
// returns a JSON string; encrypting it, and turning a throw into the
// "couldn't decrypt" placeholder, is the caller's job (see JournalEntryPayload
// for why parsing deliberately doesn't swallow errors).
//
// None of these may change shape: rows already in the database are encrypted
// with exactly these property names.

/// <summary>One logged therapy session: when it happened, and what came up.</summary>
public record TherapySession(DateOnly Date, string Notes);

/// <summary>
/// A goal as read back from storage. <see cref="LegacyEnd"/> is the target date
/// of goals written before <c>end_date</c> became a real column; callers prefer
/// the column and fall back to this. New goals never carry it — see
/// <see cref="GoalPayload.Serialize"/>.
/// </summary>
public record GoalContent(string Title, DateOnly? LegacyEnd);

/// <summary>One progress note logged against a goal.</summary>
public record GoalProgressNote(DateOnly Date, string Note);

/// <summary>Payload codec for <c>therapy_sessions</c>: <c>{"date":"yyyy-MM-dd","notes":…}</c>.</summary>
public static class TherapySessionPayload
{
    private const string DateFormat = "yyyy-MM-dd";

    public static string Serialize(TherapySession s) => JsonSerializer.Serialize(new
    {
        date = s.Date.ToString(DateFormat),
        notes = s.Notes,
    });

    /// <param name="fallbackDate">Used when the payload has no usable date — the page passes today.</param>
    public static TherapySession Parse(string json, DateOnly fallbackDate)
    {
        using var doc = JsonDocument.Parse(json);
        var e = doc.RootElement;
        return new TherapySession(
            e.TryGetProperty("date", out var dt) && DateOnly.TryParse(dt.GetString(), out var d) ? d : fallbackDate,
            e.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : "");
    }
}

/// <summary>
/// Payload codec for <c>goals</c>: <c>{"title":…}</c>.
/// <para>
/// Reads may also see a legacy <c>{"end":"yyyy-MM-dd"}</c>. Writes deliberately
/// never emit it — the <c>end_date</c> column is authoritative now — which is
/// why <see cref="Serialize"/> takes only a title rather than a
/// <see cref="GoalContent"/>: there is no end date to drop.
/// </para>
/// </summary>
public static class GoalPayload
{
    public static string Serialize(string title) => JsonSerializer.Serialize(new { title });

    public static GoalContent Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var e = doc.RootElement;
        return new GoalContent(
            e.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
            e.TryGetProperty("end", out var end) && end.ValueKind == JsonValueKind.String
                && DateOnly.TryParse(end.GetString(), out var d) ? d : null);
    }
}

/// <summary>Payload codec for <c>goal_progress</c>: <c>{"date":"yyyy-MM-dd","note":…}</c>.</summary>
public static class GoalProgressPayload
{
    private const string DateFormat = "yyyy-MM-dd";

    public static string Serialize(GoalProgressNote p) => JsonSerializer.Serialize(new
    {
        date = p.Date.ToString(DateFormat),
        note = p.Note,
    });

    /// <param name="fallbackDate">Used when the payload has no usable date — the page passes today.</param>
    public static GoalProgressNote Parse(string json, DateOnly fallbackDate)
    {
        using var doc = JsonDocument.Parse(json);
        var e = doc.RootElement;
        return new GoalProgressNote(
            e.TryGetProperty("date", out var dt) && DateOnly.TryParse(dt.GetString(), out var d) ? d : fallbackDate,
            e.TryGetProperty("note", out var n) ? n.GetString() ?? "" : "");
    }
}
