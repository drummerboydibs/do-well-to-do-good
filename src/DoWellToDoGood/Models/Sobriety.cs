using System.Text.Json;

namespace DoWellToDoGood.Models;

/// <summary>
/// One recovery counter's decrypted contents: what's being left behind, when the
/// current run started, and the history banked by past fresh starts.
/// <see cref="BestStreakDays"/> and <see cref="LastRunDays"/> are the runs as of
/// the last reset — the run in progress is measured from <see cref="CleanSince"/>,
/// so the page takes the larger of the two when it shows a longest run.
/// </summary>
public record SobrietyCounter(
    string Name,
    DateOnly CleanSince,
    int BestStreakDays,
    int LastRunDays,
    int ResetCount);

/// <summary>
/// Payload codec for <c>sobriety_counters</c>:
/// <c>{"name":…,"cleanSince":"yyyy-MM-dd","best":N,"last":N,"resets":N}</c>.
/// <para>
/// The codec takes and returns plaintext JSON; encrypting it, and turning a throw
/// into the "couldn't decrypt" placeholder, is the caller's job. Unlike the other
/// tables there are no structural columns to fall back on — the whole counter
/// lives in here — so the shape may not change: counters already in the database
/// are encrypted with exactly these property names.
/// </para>
/// </summary>
public static class SobrietyCounterPayload
{
    private const string DateFormat = "yyyy-MM-dd";

    public static string Serialize(SobrietyCounter c) => JsonSerializer.Serialize(new
    {
        name = c.Name,
        cleanSince = c.CleanSince.ToString(DateFormat),
        best = c.BestStreakDays,
        last = c.LastRunDays,
        resets = c.ResetCount,
    });

    /// <param name="fallbackDate">Used when the payload has no usable clean-since date — callers pass today.</param>
    public static SobrietyCounter Parse(string json, DateOnly fallbackDate)
    {
        using var doc = JsonDocument.Parse(json);
        var e = doc.RootElement;
        return new SobrietyCounter(
            e.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            e.TryGetProperty("cleanSince", out var cs) && DateOnly.TryParse(cs.GetString(), out var d) ? d : fallbackDate,
            ReadCount(e, "best"),
            ReadCount(e, "last"),
            ReadCount(e, "resets"));
    }

    private static int ReadCount(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.TryGetInt32(out var i) ? i : 0;
}
