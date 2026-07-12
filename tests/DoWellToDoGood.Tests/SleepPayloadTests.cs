using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class SleepPayloadTests
{
    private static readonly DateOnly Night = new(2026, 7, 12);

    [Fact]
    public void RoundTrips_AllFields()
    {
        var original = new SleepNight(
            Night,
            new TimeOnly(23, 15),
            new TimeOnly(7, 0),
            new[] { 20, 25 },
            new[] { 30 },
            "busy mind");

        var parsed = SleepPayload.Parse(SleepPayload.Serialize(original), Night);

        Assert.Equal(Night, parsed.Night);
        Assert.Equal(new TimeOnly(23, 15), parsed.Bedtime);
        Assert.Equal(new TimeOnly(7, 0), parsed.WakeTime);
        Assert.Equal(new[] { 20, 25 }, parsed.Wakeups);
        Assert.Equal(new[] { 30 }, parsed.Naps);
        Assert.Equal("busy mind", parsed.Thoughts);
    }

    [Fact]
    public void RoundTrips_EmptyNight()
    {
        var parsed = SleepPayload.Parse(SleepPayload.Serialize(SleepNight.Empty(Night)), Night);

        Assert.Null(parsed.Bedtime);
        Assert.Null(parsed.WakeTime);
        Assert.Empty(parsed.Wakeups);
        Assert.Empty(parsed.Naps);
        Assert.Equal("", parsed.Thoughts);
    }

    [Fact]
    public void Parse_Garbage_ReturnsEmptyNight_KeepingDate()
    {
        var parsed = SleepPayload.Parse("this is not json", Night);

        Assert.Equal(Night, parsed.Night);
        Assert.Null(parsed.Bedtime);
        Assert.Empty(parsed.Wakeups);
        Assert.Equal("", parsed.Thoughts);
    }

    [Fact]
    public void Parse_MissingFields_FallBackToDefaults()
    {
        var parsed = SleepPayload.Parse("{\"thoughts\":\"only this\"}", Night);

        Assert.Null(parsed.Bedtime);
        Assert.Null(parsed.WakeTime);
        Assert.Empty(parsed.Wakeups);
        Assert.Empty(parsed.Naps);
        Assert.Equal("only this", parsed.Thoughts);
    }

    [Theory]
    [InlineData("\"25:00\"")]  // out of range
    [InlineData("\"abc\"")]    // not a time
    [InlineData("\"7:00\"")]   // wrong format (needs HH:mm)
    [InlineData("null")]
    public void Parse_BadBedtime_IsIgnored(string bedtimeJson)
    {
        var parsed = SleepPayload.Parse($"{{\"bedtime\":{bedtimeJson}}}", Night);
        Assert.Null(parsed.Bedtime);
    }

    [Fact]
    public void Parse_DropsNonPositiveMinutes()
    {
        // Only strictly-positive durations are meaningful; 0 and negatives are noise.
        var parsed = SleepPayload.Parse("{\"wakeups\":[0,-5,20,15],\"naps\":[-1,45]}", Night);

        Assert.Equal(new[] { 20, 15 }, parsed.Wakeups);
        Assert.Equal(new[] { 45 }, parsed.Naps);
    }

    [Fact]
    public void Serialize_OmitsTimesWhenNull()
    {
        var json = SleepPayload.Serialize(SleepNight.Empty(Night));
        // Null times serialize as JSON null, which Parse treats as "not set".
        var parsed = SleepPayload.Parse(json, Night);
        Assert.Null(parsed.Bedtime);
        Assert.Null(parsed.WakeTime);
    }
}
