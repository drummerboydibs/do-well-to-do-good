using System.Text.Json;

namespace DoWellToDoGood.Models;

/// <summary>
/// A journal entry's plaintext: the words, and the feeling they were named
/// with. The id and timestamps live in their own columns — everything here is
/// what gets encrypted before it leaves the browser.
/// </summary>
public record JournalEntry(string Body, string Emotion);

/// <summary>
/// Encrypted-payload (de)serialisation for a <see cref="JournalEntry"/>.
/// <para>
/// The wire shape is <c>{"body":…,"emotion":…}</c> and must stay exactly that:
/// entries already saved are encrypted with these property names, and a rename
/// would strand them.
/// </para>
/// <para>
/// Unlike <see cref="SleepPayload"/>, <see cref="Parse"/> does not swallow bad
/// input. Callers decrypt and parse under one try/catch, so a throw is how an
/// unreadable payload becomes the "couldn't be decrypted" placeholder — which
/// also hides Edit, so a save can't overwrite ciphertext we can't recover.
/// Defaulting a missing <c>body</c> to "" would instead offer an edit that
/// silently erases the entry. <c>emotion</c> is genuinely optional and does
/// default to "".
/// </para>
/// </summary>
public static class JournalEntryPayload
{
    public static string Serialize(JournalEntry e) => JsonSerializer.Serialize(new
    {
        body = e.Body,
        emotion = e.Emotion,
    });

    public static JournalEntry Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var e = doc.RootElement;
        return new JournalEntry(
            e.GetProperty("body").GetString() ?? "",
            e.TryGetProperty("emotion", out var em) ? em.GetString() ?? "" : "");
    }
}
