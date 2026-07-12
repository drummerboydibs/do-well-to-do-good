using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class SleepMathTests
{
    private static SleepNight Night(
        TimeOnly? bed = null, TimeOnly? wake = null,
        int[]? wakeups = null, int[]? naps = null, string thoughts = "") =>
        new(new DateOnly(2026, 7, 12), bed, wake, wakeups ?? Array.Empty<int>(), naps ?? Array.Empty<int>(), thoughts);

    // ---- MinutesInBed ----

    [Fact]
    public void MinutesInBed_SameEvening_ToMorning()
    {
        Assert.Equal(480, SleepMath.MinutesInBed(new(22, 0), new(6, 0)));   // 8h across midnight
    }

    [Fact]
    public void MinutesInBed_CrossesMidnight()
    {
        Assert.Equal(465, SleepMath.MinutesInBed(new(23, 15), new(7, 0)));  // 7h45
    }

    [Fact]
    public void MinutesInBed_ShortWindowSameNight()
    {
        Assert.Equal(30, SleepMath.MinutesInBed(new(22, 0), new(22, 30)));
    }

    [Fact]
    public void MinutesInBed_EqualTimes_IsFullDay()
    {
        // Bed and wake identical means a full 24h wrap rather than zero.
        Assert.Equal(1440, SleepMath.MinutesInBed(new(7, 0), new(7, 0)));
    }

    // ---- Per-night metrics ----

    [Fact]
    public void TimeInBed_IsNull_WhenEitherTimeMissing()
    {
        Assert.Null(SleepMath.TimeInBedMinutes(Night(bed: new(23, 0))));
        Assert.Null(SleepMath.TimeInBedMinutes(Night(wake: new(7, 0))));
        Assert.Null(SleepMath.TimeInBedMinutes(Night()));
    }

    [Fact]
    public void EstimatedSleep_SubtractsNightWakeups()
    {
        var n = Night(new(23, 0), new(7, 0), wakeups: new[] { 20, 25 }); // 480 - 45
        Assert.Equal(435, SleepMath.EstimatedSleepMinutes(n));
    }

    [Fact]
    public void EstimatedSleep_NeverNegative()
    {
        var n = Night(new(2, 0), new(3, 0), wakeups: new[] { 120 }); // 60 in bed, 120 awake
        Assert.Equal(0, SleepMath.EstimatedSleepMinutes(n));
    }

    [Fact]
    public void EstimatedSleep_IsNull_WithoutTimes()
    {
        Assert.Null(SleepMath.EstimatedSleepMinutes(Night(wakeups: new[] { 30 })));
    }

    [Fact]
    public void WakeAndNapTotals_Sum()
    {
        var n = Night(wakeups: new[] { 10, 15, 5 }, naps: new[] { 20, 40 });
        Assert.Equal(30, SleepMath.TotalWakeMinutes(n));
        Assert.Equal(60, SleepMath.TotalNapMinutes(n));
    }

    // ---- Summarize ----

    [Fact]
    public void Summarize_Empty_IsAllZeroOrNull()
    {
        var s = SleepMath.Summarize(Array.Empty<SleepNight>());
        Assert.Equal(0, s.NightsLogged);
        Assert.Null(s.AvgSleepMinutes);
        Assert.Null(s.AvgTimeInBedMinutes);
        Assert.Equal(0.0, s.AvgWakeupsPerNight);
        Assert.Equal(0, s.NapCount);
        Assert.Equal(0, s.TotalNapMinutes);
    }

    [Fact]
    public void Summarize_AveragesSleepOnlyOverNightsWithTimes()
    {
        var nights = new[]
        {
            Night(new(23, 0), new(7, 0)),                 // 480 sleep
            Night(new(23, 0), new(5, 0)),                 // 360 sleep
            Night(thoughts: "couldn't settle"),           // no times -> excluded from sleep avg
        };
        var s = SleepMath.Summarize(nights);

        Assert.Equal(3, s.NightsLogged);
        Assert.Equal(420.0, s.AvgSleepMinutes);           // (480 + 360) / 2, not / 3
        Assert.Equal(420.0, s.AvgTimeInBedMinutes);
    }

    [Fact]
    public void Summarize_WakeupsAveragedAcrossAllNights_AndNapsTotalled()
    {
        var nights = new[]
        {
            Night(wakeups: new[] { 10, 10 }, naps: new[] { 30 }),
            Night(),                                       // zero wake-ups still counts in the average
        };
        var s = SleepMath.Summarize(nights);

        Assert.Equal(1.0, s.AvgWakeupsPerNight);          // (2 + 0) / 2
        Assert.Equal(1, s.NapCount);
        Assert.Equal(30, s.TotalNapMinutes);
    }

    // ---- FormatDuration ----

    [Theory]
    [InlineData(435, "7h 15m")]
    [InlineData(60, "1h")]
    [InlineData(90, "1h 30m")]
    [InlineData(40, "40m")]
    [InlineData(0, "0m")]
    [InlineData(-5, "0m")]
    public void FormatDuration_ReadsNaturally(int minutes, string expected)
    {
        Assert.Equal(expected, SleepMath.FormatDuration(minutes));
    }

    // ---- WeekDays ----

    [Fact]
    public void WeekDays_AreSevenConsecutive_MondayFirst_ContainingAnchor()
    {
        var anchor = new DateOnly(2026, 7, 15); // a Wednesday
        var days = SleepMath.WeekDays(anchor);

        Assert.Equal(7, days.Count);
        Assert.Equal(DayOfWeek.Monday, days[0].DayOfWeek);
        Assert.Contains(anchor, days);
        for (var i = 1; i < days.Count; i++)
            Assert.Equal(days[i - 1].AddDays(1), days[i]);
    }

    [Fact]
    public void WeekDays_WhenAnchorIsMonday_StartsThere()
    {
        var monday = new DateOnly(2026, 7, 13);
        Assert.Equal(monday, SleepMath.WeekDays(monday)[0]);
    }

    [Fact]
    public void WeekDays_WhenAnchorIsSunday_SundayIsLast()
    {
        var sunday = new DateOnly(2026, 7, 19);
        var days = SleepMath.WeekDays(sunday);
        Assert.Equal(sunday, days[6]);
        Assert.Equal(DayOfWeek.Monday, days[0].DayOfWeek);
    }
}
