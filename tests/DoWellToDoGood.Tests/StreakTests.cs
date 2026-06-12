using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class StreakTests
{
    // CurrentStreak anchors to the machine's local "today", so derive fixtures from it.
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    [Fact]
    public void EmptyHistory_IsZero()
    {
        Assert.Equal(0, StatsService.CurrentStreak(Array.Empty<DateOnly>()));
    }

    [Fact]
    public void TodayOnly_IsOne()
    {
        Assert.Equal(1, StatsService.CurrentStreak(new[] { Today }));
    }

    [Fact]
    public void ConsecutiveDaysEndingToday_CountAll()
    {
        var days = new[] { Today, Today.AddDays(-1), Today.AddDays(-2) };
        Assert.Equal(3, StatsService.CurrentStreak(days));
    }

    [Fact]
    public void StreakEndingYesterday_StillCounts()
    {
        // No activity today yet — the streak survives on yesterday's anchor.
        var days = new[] { Today.AddDays(-1), Today.AddDays(-2) };
        Assert.Equal(2, StatsService.CurrentStreak(days));
    }

    [Fact]
    public void GapBreaksTheStreak()
    {
        // Today + yesterday are present, then a gap; only the run ending today counts.
        var days = new[] { Today, Today.AddDays(-1), Today.AddDays(-3), Today.AddDays(-4) };
        Assert.Equal(2, StatsService.CurrentStreak(days));
    }

    [Fact]
    public void NeitherTodayNorYesterday_IsZero()
    {
        var days = new[] { Today.AddDays(-2), Today.AddDays(-3) };
        Assert.Equal(0, StatsService.CurrentStreak(days));
    }

    [Fact]
    public void DuplicateDays_AreCountedOnce()
    {
        var days = new[] { Today, Today, Today.AddDays(-1), Today.AddDays(-1) };
        Assert.Equal(2, StatsService.CurrentStreak(days));
    }

    [Fact]
    public void UnorderedInput_IsHandled()
    {
        var days = new[] { Today.AddDays(-2), Today, Today.AddDays(-1) };
        Assert.Equal(3, StatsService.CurrentStreak(days));
    }

    [Fact]
    public void FutureDays_DoNotInflateStreak()
    {
        // A clock-skewed future day shouldn't extend a streak anchored at today.
        var days = new[] { Today.AddDays(2), Today, Today.AddDays(-1) };
        Assert.Equal(2, StatsService.CurrentStreak(days));
    }
}
