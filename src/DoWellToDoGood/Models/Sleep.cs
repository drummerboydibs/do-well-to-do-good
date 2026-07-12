using System.Text.Json;

namespace DoWellToDoGood.Models;

/// <summary>
/// One night's sleep, as logged the next morning. <see cref="Night"/> is the
/// date the sleep is attributed to (the morning you woke). All fields are
/// optional so a hurried entry still counts — you can log just a bedtime and
/// wake time, or add night wake-ups, daytime naps, and what was on your mind.
/// Times are local wall-clock (a bedtime after a wake time simply means the
/// night crossed midnight). Durations are whole minutes.
/// </summary>
public record SleepNight(
    DateOnly Night,
    TimeOnly? Bedtime,
    TimeOnly? WakeTime,
    IReadOnlyList<int> Wakeups,   // minutes spent awake for each night waking
    IReadOnlyList<int> Naps,      // minutes for each daytime nap
    string Thoughts)
{
    public static SleepNight Empty(DateOnly night) =>
        new(night, null, null, Array.Empty<int>(), Array.Empty<int>(), "");
}

/// <summary>
/// Encrypted-payload (de)serialisation for a <see cref="SleepNight"/>. The
/// night's date lives in its own column (needed to order and edit nights), so
/// it is deliberately NOT part of the payload — everything here is what gets
/// encrypted before it leaves the browser. Parsing is resilient: missing or
/// malformed fields fall back to empty rather than throwing.
/// </summary>
public static class SleepPayload
{
    private const string TimeFormat = "HH:mm";

    public static string Serialize(SleepNight n) => JsonSerializer.Serialize(new
    {
        bedtime = n.Bedtime?.ToString(TimeFormat),
        wakeTime = n.WakeTime?.ToString(TimeFormat),
        wakeups = n.Wakeups,
        naps = n.Naps,
        thoughts = n.Thoughts,
    });

    public static SleepNight Parse(string json, DateOnly night)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var e = doc.RootElement;
            return new SleepNight(
                night,
                ReadTime(e, "bedtime"),
                ReadTime(e, "wakeTime"),
                ReadMinutes(e, "wakeups"),
                ReadMinutes(e, "naps"),
                e.TryGetProperty("thoughts", out var t) ? t.GetString() ?? "" : "");
        }
        catch
        {
            return SleepNight.Empty(night);
        }
    }

    private static TimeOnly? ReadTime(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v)
        && v.ValueKind == JsonValueKind.String
        && TimeOnly.TryParseExact(v.GetString(), TimeFormat, out var parsed)
            ? parsed
            : null;

    private static IReadOnlyList<int> ReadMinutes(JsonElement e, string name)
    {
        if (!e.TryGetProperty(name, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return Array.Empty<int>();
        var list = new List<int>();
        foreach (var item in arr.EnumerateArray())
            if (item.TryGetInt32(out var m) && m > 0)
                list.Add(m);
        return list;
    }
}

/// <summary>Aggregate view of a set of nights, for the "at a glance" reviews.</summary>
public readonly record struct SleepStats(
    int NightsLogged,
    double? AvgSleepMinutes,
    double? AvgTimeInBedMinutes,
    double AvgWakeupsPerNight,
    int NapCount,
    int TotalNapMinutes);

/// <summary>
/// Pure sleep arithmetic — time in bed (crossing midnight), estimated sleep
/// (time in bed minus night wake-ups), nap totals, and week/month aggregates.
/// No storage, no crypto, no clock: everything is a function of its inputs so
/// it can be unit-tested directly.
/// </summary>
public static class SleepMath
{
    private const int MinutesPerDay = 24 * 60;

    /// <summary>
    /// Minutes between going to bed and getting up, wrapping past midnight. A
    /// wake time at or before the bedtime is treated as the next day.
    /// </summary>
    public static int MinutesInBed(TimeOnly bedtime, TimeOnly wakeTime)
    {
        var mins = (int)(wakeTime.ToTimeSpan() - bedtime.ToTimeSpan()).TotalMinutes;
        return mins <= 0 ? mins + MinutesPerDay : mins;
    }

    public static int TotalWakeMinutes(SleepNight n) => Sum(n.Wakeups);
    public static int TotalNapMinutes(SleepNight n) => Sum(n.Naps);

    /// <summary>Time in bed, or null if either the bedtime or wake time is missing.</summary>
    public static int? TimeInBedMinutes(SleepNight n) =>
        n.Bedtime is { } b && n.WakeTime is { } w ? MinutesInBed(b, w) : null;

    /// <summary>
    /// Estimated actual sleep: time in bed less the minutes spent awake during
    /// the night, never below zero. Null if there's no time-in-bed to work from.
    /// </summary>
    public static int? EstimatedSleepMinutes(SleepNight n) =>
        TimeInBedMinutes(n) is { } tib ? Math.Max(0, tib - TotalWakeMinutes(n)) : null;

    public static SleepStats Summarize(IEnumerable<SleepNight> nights)
    {
        var list = nights as IReadOnlyList<SleepNight> ?? nights.ToList();
        if (list.Count == 0)
            return new SleepStats(0, null, null, 0, 0, 0);

        var sleepMins = list.Select(EstimatedSleepMinutes).Where(m => m is not null).Select(m => m!.Value).ToList();
        var bedMins = list.Select(TimeInBedMinutes).Where(m => m is not null).Select(m => m!.Value).ToList();
        var naps = list.SelectMany(n => n.Naps).ToList();

        return new SleepStats(
            NightsLogged: list.Count,
            AvgSleepMinutes: sleepMins.Count > 0 ? sleepMins.Average() : null,
            AvgTimeInBedMinutes: bedMins.Count > 0 ? bedMins.Average() : null,
            AvgWakeupsPerNight: list.Average(n => n.Wakeups.Count),
            NapCount: naps.Count,
            TotalNapMinutes: Sum(naps));
    }

    /// <summary>Human-friendly duration, e.g. 435 → "7h 15m", 40 → "40m", 0 → "0m".</summary>
    public static string FormatDuration(int minutes)
    {
        if (minutes < 0) minutes = 0;
        var h = minutes / 60;
        var m = minutes % 60;
        return h == 0 ? $"{m}m" : m == 0 ? $"{h}h" : $"{h}h {m}m";
    }

    /// <summary>The seven dates of the week containing <paramref name="anchor"/>, Monday-first.</summary>
    public static IReadOnlyList<DateOnly> WeekDays(DateOnly anchor)
    {
        var offset = ((int)anchor.DayOfWeek + 6) % 7; // Monday = 0
        var monday = anchor.AddDays(-offset);
        return Enumerable.Range(0, 7).Select(monday.AddDays).ToList();
    }

    private static int Sum(IReadOnlyList<int> xs)
    {
        var total = 0;
        for (var i = 0; i < xs.Count; i++) total += xs[i];
        return total;
    }
}
