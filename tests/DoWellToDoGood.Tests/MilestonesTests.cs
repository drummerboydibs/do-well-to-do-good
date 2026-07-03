using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class MilestonesTests
{
    // ---- DaysClean ----

    [Fact]
    public void DaysClean_SameDay_IsZero()
    {
        var d = new DateOnly(2026, 7, 2);
        Assert.Equal(0, Milestones.DaysClean(d, d));
    }

    [Fact]
    public void DaysClean_CountsWholeDays()
    {
        Assert.Equal(10, Milestones.DaysClean(new DateOnly(2026, 6, 22), new DateOnly(2026, 7, 2)));
    }

    [Fact]
    public void DaysClean_FutureStart_ClampsToZero()
    {
        // A clean-since date in the future (e.g. clock skew) must never go negative.
        Assert.Equal(0, Milestones.DaysClean(new DateOnly(2026, 7, 5), new DateOnly(2026, 7, 2)));
    }

    // ---- IsMilestone ----

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(365)]
    [InlineData(730)]
    [InlineData(1095)]   // 3 years
    [InlineData(1460)]   // 4 years
    public void IsMilestone_True_OnLadderAndYearlyAnniversaries(int days)
    {
        Assert.True(Milestones.IsMilestone(days));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]   // between year 2 and year 3
    public void IsMilestone_False_OffLadder(int days)
    {
        Assert.False(Milestones.IsMilestone(days));
    }

    // ---- Next / DaysUntilNext ----

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 7)]
    [InlineData(89, 90)]
    [InlineData(90, 180)]
    [InlineData(365, 547)]
    [InlineData(730, 1095)]   // first yearly step past the fixed ladder
    [InlineData(800, 1095)]
    [InlineData(1095, 1460)]
    public void Next_ReturnsTheStrictlyGreaterThreshold(int days, int expected)
    {
        Assert.Equal(expected, Milestones.Next(days));
    }

    [Fact]
    public void Next_IsAlwaysStrictlyGreater_EvenOnAMilestone()
    {
        // Standing exactly on a milestone should point at the *following* one.
        Assert.True(Milestones.Next(90) > 90);
        Assert.True(Milestones.Next(365) > 365);
    }

    [Fact]
    public void DaysUntilNext_IsTheGap()
    {
        Assert.Equal(1, Milestones.DaysUntilNext(89));    // 90 - 89
        Assert.Equal(182, Milestones.DaysUntilNext(365)); // 547 - 365
    }

    // ---- Label ----

    [Theory]
    [InlineData(1, "1 day")]
    [InlineData(7, "1 week")]
    [InlineData(30, "30 days")]
    [InlineData(180, "6 months")]
    [InlineData(365, "1 year")]
    [InlineData(730, "2 years")]
    [InlineData(1095, "3 years")]
    public void Label_ReadsNaturally(int days, string expected)
    {
        Assert.Equal(expected, Milestones.Label(days));
    }
}
