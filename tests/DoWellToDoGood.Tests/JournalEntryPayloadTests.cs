using System.Text.Json;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class JournalEntryPayloadTests
{
    [Fact]
    public void RoundTrips_BodyAndEmotion()
    {
        var original = new JournalEntry("Rough morning, better by lunch.", "anxious");

        var parsed = JournalEntryPayload.Parse(JournalEntryPayload.Serialize(original));

        Assert.Equal("Rough morning, better by lunch.", parsed.Body);
        Assert.Equal("anxious", parsed.Emotion);
    }

    [Fact]
    public void RoundTrips_MultilineBody()
    {
        var original = new JournalEntry("line one\n\nline two\twith a tab", "");

        var parsed = JournalEntryPayload.Parse(JournalEntryPayload.Serialize(original));

        Assert.Equal("line one\n\nline two\twith a tab", parsed.Body);
        Assert.Equal("", parsed.Emotion);
    }

    // Entries already saved are encrypted with these exact property names, so the
    // wire shape is a contract, not an implementation detail. Round-trip tests
    // alone wouldn't catch a rename — these two do.
    [Fact]
    public void Serialize_WritesTheStoredWireShape()
    {
        var json = JournalEntryPayload.Serialize(new JournalEntry("hi", "calm"));

        Assert.Equal("{\"body\":\"hi\",\"emotion\":\"calm\"}", json);
    }

    [Fact]
    public void Parse_ReadsTheStoredWireShape()
    {
        var parsed = JournalEntryPayload.Parse("{\"body\":\"hi\",\"emotion\":\"calm\"}");

        Assert.Equal("hi", parsed.Body);
        Assert.Equal("calm", parsed.Emotion);
    }

    [Fact]
    public void Parse_MissingEmotion_DefaultsToEmpty()
    {
        var parsed = JournalEntryPayload.Parse("{\"body\":\"just words\"}");

        Assert.Equal("just words", parsed.Body);
        Assert.Equal("", parsed.Emotion);
    }

    [Fact]
    public void Parse_NullEmotion_DefaultsToEmpty()
    {
        var parsed = JournalEntryPayload.Parse("{\"body\":\"just words\",\"emotion\":null}");

        Assert.Equal("", parsed.Emotion);
    }

    // A payload with no body is unreadable, not an empty entry. Throwing is what
    // makes the page show its "couldn't be decrypted" placeholder and hide Edit;
    // defaulting to "" would offer an edit that silently erases the entry.
    [Fact]
    public void Parse_MissingBody_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => JournalEntryPayload.Parse("{\"emotion\":\"calm\"}"));
    }

    [Fact]
    public void Parse_Garbage_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => JournalEntryPayload.Parse("this is not json"));
    }
}
