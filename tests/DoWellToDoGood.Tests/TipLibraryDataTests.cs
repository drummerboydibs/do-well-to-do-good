using System.Reflection;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

/// <summary>
/// Data-integrity guards over the whole tip library. As the library keeps
/// growing, these catch the mistakes that are easy to make by hand: a missing
/// citation, a broken source link, a blank tip, or a duplicated id (which would
/// quietly break the "don't repeat recently" tracking, since it keys on id).
///
/// Pools are private static <see cref="Tip"/>[] fields; we gather them by
/// reflection so a newly added pool is covered automatically without touching
/// this test.
/// </summary>
public class TipLibraryDataTests
{
    private static IReadOnlyList<Tip> AllTips { get; } =
        typeof(TipLibrary)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Tip[]))
            .SelectMany(f => (Tip[])f.GetValue(null)!)
            .ToList();

    [Fact]
    public void Reflection_ActuallyFoundThePools()
    {
        // If this ever hits zero the reflection above has silently stopped
        // finding pools, which would make every other test here vacuously pass.
        Assert.True(AllTips.Count >= 40, $"Only found {AllTips.Count} tips — did the pool shape change?");
    }

    [Fact]
    public void EveryTip_HasIdTextAndCitation()
    {
        Assert.All(AllTips, t =>
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Id), "A tip has a blank id.");
            Assert.False(string.IsNullOrWhiteSpace(t.Text), $"Tip '{t.Id}' has blank text.");
            Assert.False(string.IsNullOrWhiteSpace(t.SourceName), $"Tip '{t.Id}' is missing a source name.");
        });
    }

    [Fact]
    public void EveryTip_LinksToAnHttpsSource()
    {
        // Stealing is bad, crediting is good: every tip must point at a real link.
        Assert.All(AllTips, t =>
        {
            Assert.StartsWith("https://", t.SourceUrl);
            Assert.True(Uri.TryCreate(t.SourceUrl, UriKind.Absolute, out _),
                $"Tip '{t.Id}' has an unparseable source URL: {t.SourceUrl}");
        });
    }

    [Fact]
    public void TipIds_AreGloballyUnique()
    {
        // TipHistoryService de-dupes recent tips by id, so ids must be unique
        // across every pool — not just within one.
        var dupes = AllTips
            .GroupBy(t => t.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.True(dupes.Count == 0, $"Duplicate tip ids: {string.Join(", ", dupes)}");
    }
}
