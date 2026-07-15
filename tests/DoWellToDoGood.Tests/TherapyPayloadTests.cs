using System.Text.Json;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class TherapySessionPayloadTests
{
    private static readonly DateOnly Fallback = new(2026, 7, 15);

    [Fact]
    public void RoundTrips_DateAndNotes()
    {
        var original = new TherapySession(new DateOnly(2026, 3, 4), "Talked about the move.");

        var parsed = TherapySessionPayload.Parse(TherapySessionPayload.Serialize(original), Fallback);

        Assert.Equal(new DateOnly(2026, 3, 4), parsed.Date);
        Assert.Equal("Talked about the move.", parsed.Notes);
    }

    // Sessions already saved are encrypted with these exact property names and
    // this exact date format — a rename or reformat strands them.
    [Fact]
    public void Serialize_WritesTheStoredWireShape()
    {
        var json = TherapySessionPayload.Serialize(new TherapySession(new DateOnly(2026, 7, 12), "went well"));

        Assert.Equal("{\"date\":\"2026-07-12\",\"notes\":\"went well\"}", json);
    }

    [Fact]
    public void Parse_ReadsTheStoredWireShape()
    {
        var parsed = TherapySessionPayload.Parse("{\"date\":\"2026-07-12\",\"notes\":\"went well\"}", Fallback);

        Assert.Equal(new DateOnly(2026, 7, 12), parsed.Date);
        Assert.Equal("went well", parsed.Notes);
    }

    [Fact]
    public void Parse_MissingNotes_DefaultsToEmpty()
    {
        var parsed = TherapySessionPayload.Parse("{\"date\":\"2026-07-12\"}", Fallback);

        Assert.Equal("", parsed.Notes);
    }

    [Theory]
    [InlineData("{}")]                        // no date at all
    [InlineData("{\"date\":\"not a date\"}")] // unparseable
    [InlineData("{\"date\":\"\"}")]
    [InlineData("{\"date\":null}")]
    public void Parse_NoUsableDate_FallsBackToTheGivenDate(string json)
    {
        var parsed = TherapySessionPayload.Parse(json, Fallback);

        Assert.Equal(Fallback, parsed.Date);
    }

    [Fact]
    public void Parse_Garbage_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => TherapySessionPayload.Parse("not json", Fallback));
    }
}

public class GoalPayloadTests
{
    [Fact]
    public void RoundTrips_Title()
    {
        var parsed = GoalPayload.Parse(GoalPayload.Serialize("Walk outside three times this week"));

        Assert.Equal("Walk outside three times this week", parsed.Title);
    }

    [Fact]
    public void Serialize_WritesTheStoredWireShape()
    {
        Assert.Equal("{\"title\":\"walk daily\"}", GoalPayload.Serialize("walk daily"));
    }

    [Fact]
    public void Parse_ReadsTheStoredWireShape()
    {
        var parsed = GoalPayload.Parse("{\"title\":\"walk daily\"}");

        Assert.Equal("walk daily", parsed.Title);
        Assert.Null(parsed.LegacyEnd);
    }

    // Goals written before end_date became a real column carry their target date
    // inside the encrypted payload; Load() prefers the column and falls back to this.
    [Fact]
    public void Parse_LegacyEnd_IsRead()
    {
        var parsed = GoalPayload.Parse("{\"title\":\"old goal\",\"end\":\"2026-09-01\"}");

        Assert.Equal("old goal", parsed.Title);
        Assert.Equal(new DateOnly(2026, 9, 1), parsed.LegacyEnd);
    }

    [Theory]
    [InlineData("{\"title\":\"g\",\"end\":\"nonsense\"}")]  // unparseable
    [InlineData("{\"title\":\"g\",\"end\":20260901}")]      // not a string
    [InlineData("{\"title\":\"g\",\"end\":null}")]
    [InlineData("{\"title\":\"g\"}")]
    public void Parse_NoUsableEnd_IsNull(string json)
    {
        Assert.Null(GoalPayload.Parse(json).LegacyEnd);
    }

    // New goals never write "end" — the end_date column is authoritative, so a
    // legacy date read on load is deliberately not written back.
    [Fact]
    public void Serialize_NeverWritesLegacyEnd()
    {
        Assert.DoesNotContain("end", GoalPayload.Serialize("walk daily"));
    }

    [Fact]
    public void Parse_MissingTitle_DefaultsToEmpty()
    {
        Assert.Equal("", GoalPayload.Parse("{\"end\":\"2026-09-01\"}").Title);
    }

    [Fact]
    public void Parse_Garbage_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => GoalPayload.Parse("not json"));
    }
}

public class GoalProgressPayloadTests
{
    private static readonly DateOnly Fallback = new(2026, 7, 15);

    [Fact]
    public void RoundTrips_DateAndNote()
    {
        var original = new GoalProgressNote(new DateOnly(2026, 5, 20), "Introduced myself to a stranger.");

        var parsed = GoalProgressPayload.Parse(GoalProgressPayload.Serialize(original), Fallback);

        Assert.Equal(new DateOnly(2026, 5, 20), parsed.Date);
        Assert.Equal("Introduced myself to a stranger.", parsed.Note);
    }

    [Fact]
    public void Serialize_WritesTheStoredWireShape()
    {
        var json = GoalProgressPayload.Serialize(new GoalProgressNote(new DateOnly(2026, 7, 12), "did it"));

        Assert.Equal("{\"date\":\"2026-07-12\",\"note\":\"did it\"}", json);
    }

    [Fact]
    public void Parse_ReadsTheStoredWireShape()
    {
        var parsed = GoalProgressPayload.Parse("{\"date\":\"2026-07-12\",\"note\":\"did it\"}", Fallback);

        Assert.Equal(new DateOnly(2026, 7, 12), parsed.Date);
        Assert.Equal("did it", parsed.Note);
    }

    [Fact]
    public void Parse_MissingNote_DefaultsToEmpty()
    {
        Assert.Equal("", GoalProgressPayload.Parse("{\"date\":\"2026-07-12\"}", Fallback).Note);
    }

    [Theory]
    [InlineData("{}")]                        // no date at all
    [InlineData("{\"date\":\"not a date\"}")] // unparseable
    [InlineData("{\"date\":\"\"}")]
    [InlineData("{\"date\":null}")]
    public void Parse_NoUsableDate_FallsBackToTheGivenDate(string json)
    {
        Assert.Equal(Fallback, GoalProgressPayload.Parse(json, Fallback).Date);
    }

    [Fact]
    public void Parse_Garbage_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => GoalProgressPayload.Parse("not json", Fallback));
    }
}
