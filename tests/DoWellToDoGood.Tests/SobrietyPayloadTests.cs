using System.Text.Json;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class SobrietyCounterPayloadTests
{
    private static readonly DateOnly Fallback = new(2026, 7, 15);

    [Fact]
    public void RoundTrips_AllFields()
    {
        var original = new SobrietyCounter("Alcohol", new DateOnly(2026, 3, 4), 90, 12, 3);

        var parsed = SobrietyCounterPayload.Parse(SobrietyCounterPayload.Serialize(original), Fallback);

        Assert.Equal(original, parsed);
    }

    [Fact]
    public void RoundTrips_FreshCounter()
    {
        // What AddCounter writes: no history banked yet.
        var original = new SobrietyCounter("Nicotine", new DateOnly(2026, 7, 15), 0, 0, 0);

        var parsed = SobrietyCounterPayload.Parse(SobrietyCounterPayload.Serialize(original), Fallback);

        Assert.Equal(original, parsed);
    }

    // Counters already saved are encrypted with these exact property names and
    // this exact date format — a rename, a reformat, or a dropped field strands
    // them. There are no structural columns to rebuild a counter from.
    [Fact]
    public void Serialize_WritesTheStoredWireShape()
    {
        var json = SobrietyCounterPayload.Serialize(
            new SobrietyCounter("Alcohol", new DateOnly(2026, 7, 12), 90, 12, 3));

        Assert.Equal(
            "{\"name\":\"Alcohol\",\"cleanSince\":\"2026-07-12\",\"best\":90,\"last\":12,\"resets\":3}",
            json);
    }

    [Fact]
    public void Parse_ReadsTheStoredWireShape()
    {
        var parsed = SobrietyCounterPayload.Parse(
            "{\"name\":\"Alcohol\",\"cleanSince\":\"2026-07-12\",\"best\":90,\"last\":12,\"resets\":3}",
            Fallback);

        Assert.Equal("Alcohol", parsed.Name);
        Assert.Equal(new DateOnly(2026, 7, 12), parsed.CleanSince);
        Assert.Equal(90, parsed.BestStreakDays);
        Assert.Equal(12, parsed.LastRunDays);
        Assert.Equal(3, parsed.ResetCount);
    }

    [Fact]
    public void Parse_MissingName_DefaultsToEmpty()
    {
        var parsed = SobrietyCounterPayload.Parse("{\"cleanSince\":\"2026-07-12\"}", Fallback);

        Assert.Equal("", parsed.Name);
    }

    [Fact]
    public void Parse_MissingCounts_DefaultToZero()
    {
        var parsed = SobrietyCounterPayload.Parse(
            "{\"name\":\"Alcohol\",\"cleanSince\":\"2026-07-12\"}", Fallback);

        Assert.Equal(0, parsed.BestStreakDays);
        Assert.Equal(0, parsed.LastRunDays);
        Assert.Equal(0, parsed.ResetCount);
    }

    [Theory]
    [InlineData("{}")]                              // no date at all
    [InlineData("{\"cleanSince\":\"not a date\"}")] // unparseable
    [InlineData("{\"cleanSince\":\"\"}")]
    [InlineData("{\"cleanSince\":null}")]
    public void Parse_NoUsableDate_FallsBackToTheGivenDate(string json)
    {
        Assert.Equal(Fallback, SobrietyCounterPayload.Parse(json, Fallback).CleanSince);
    }

    [Fact]
    public void Parse_Garbage_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => SobrietyCounterPayload.Parse("not json", Fallback));
    }
}
