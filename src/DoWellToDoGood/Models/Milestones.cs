namespace DoWellToDoGood.Models;

/// <summary>
/// Recovery milestones — an AA-style ladder (24 hours, a week, two weeks, a
/// month, 60 &amp; 90 days, 6 &amp; 9 months, a year, 18 months, two years, then each
/// further year). Every value is a pure function of a day count, so the UI keeps
/// no milestone state and the whole thing is trivially testable.
/// </summary>
public static class Milestones
{
    // Fixed thresholds through two years; beyond that, every additional year.
    private static readonly int[] Ladder = { 1, 7, 14, 30, 60, 90, 180, 270, 365, 547, 730 };
    private const int Year = 365;
    private const int TwoYears = 730;

    /// <summary>Whole days clean between a start date and a reference day (never negative).</summary>
    public static int DaysClean(DateOnly cleanSince, DateOnly today)
        => Math.Max(0, today.DayNumber - cleanSince.DayNumber);

    /// <summary>True when a day count lands exactly on a milestone.</summary>
    public static bool IsMilestone(int days)
    {
        if (days <= 0) return false;
        if (Ladder.Contains(days)) return true;
        return days > TwoYears && (days - TwoYears) % Year == 0;
    }

    /// <summary>The next milestone threshold strictly greater than <paramref name="days"/>.</summary>
    public static int Next(int days)
    {
        foreach (var d in Ladder)
            if (d > days) return d;
        // Past two years: the next yearly anniversary.
        var extraYears = (days - TwoYears) / Year + 1;
        return TwoYears + extraYears * Year;
    }

    /// <summary>Days remaining until the next milestone.</summary>
    public static int DaysUntilNext(int days) => Next(days) - days;

    /// <summary>A human label for a milestone threshold, e.g. 7 → "1 week", 365 → "1 year".</summary>
    public static string Label(int days) => days switch
    {
        1 => "1 day",
        7 => "1 week",
        14 => "2 weeks",
        30 => "30 days",
        60 => "60 days",
        90 => "90 days",
        180 => "6 months",
        270 => "9 months",
        365 => "1 year",
        547 => "18 months",
        730 => "2 years",
        _ => $"{days / Year} years",
    };
}
