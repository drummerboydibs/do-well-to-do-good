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

    // A count that isn't a whole number is simply not usable, and JsonElement
    // reports that by returning false rather than throwing — so, unlike the
    // wrong-typed cases below, it falls back to zero.
    [Fact]
    public void Parse_NonIntegerCount_FallsBackToZero()
    {
        Assert.Equal(0, SobrietyCounterPayload.Parse("{\"resets\":1.5}", Fallback).ResetCount);
    }

    // Throwing on a wrong-typed property is load-bearing, not incidental: it is
    // what makes a corrupt counter surface as the Recovery page's "couldn't
    // decrypt" placeholder and drop out of the SobrietyCounters list, instead of
    // rendering as a plausible-looking counter with the field silently zeroed.
    // A "lenient" refactor of Parse would quietly change both. Note these are
    // InvalidOperationException, not JsonException — the JSON is well-formed.
    [Theory]
    [InlineData("{\"cleanSince\":20260712}")] // date as a number
    [InlineData("{\"name\":123}")]            // name as a number
    [InlineData("{\"best\":\"90\"}")]         // count as a string
    [InlineData("{\"last\":null}")]           // count as null
    [InlineData("{\"resets\":true}")]         // count as a bool
    public void Parse_WrongTypedProperty_Throws(string json)
    {
        Assert.Throws<InvalidOperationException>(() => SobrietyCounterPayload.Parse(json, Fallback));
    }

    [Fact]
    public void Parse_Garbage_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => SobrietyCounterPayload.Parse("not json", Fallback));
    }
}
