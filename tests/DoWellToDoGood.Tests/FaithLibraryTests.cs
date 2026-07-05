using System.Reflection;
using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class FaithLibraryTests
{
    private const int Iterations = 200;

    // Gather every passage across all pools by reflection, so new pools are
    // covered automatically (same approach as the tip-library data test).
    private static IReadOnlyList<Passage> AllPassages { get; } =
        typeof(FaithLibrary)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Passage[]))
            .SelectMany(f => (Passage[])f.GetValue(null)!)
            .ToList();

    // ---- Data integrity ----

    [Fact]
    public void EveryPassage_IsFullyCited()
    {
        Assert.NotEmpty(AllPassages);
        Assert.All(AllPassages, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Text), $"{p.Id} has no text.");
            Assert.False(string.IsNullOrWhiteSpace(p.Reference), $"{p.Id} has no reference (book/chapter/verse).");
            Assert.False(string.IsNullOrWhiteSpace(p.Version), $"{p.Id} has no version.");
            Assert.False(string.IsNullOrWhiteSpace(p.SourceName), $"{p.Id} has no source name.");
            Assert.StartsWith("https://", p.SourceUrl);
        });
    }

    [Fact]
    public void PassageIds_AreGloballyUnique()
    {
        var dupes = AllPassages.GroupBy(p => p.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.True(dupes.Count == 0, $"Duplicate passage ids: {string.Join(", ", dupes)}");
    }

    // ---- PickForEmotion cascade ----

    [Fact]
    public void PickForEmotion_NullEmotion_ReturnsGeneralPassage()
    {
        for (var i = 0; i < Iterations; i++)
        {
            var p = FaithLibrary.PickForEmotion(FaithTradition.Christianity, null);
            Assert.NotNull(p);
            Assert.StartsWith("chr-gen-", p!.Id);
        }
    }

    [Fact]
    public void PickForEmotion_CoreFeeling_DrawsFromThatCoresPool()
    {
        // "Anxious" is under the Fearful core, which maps to the fear pool.
        var anxious = EmotionCatalog.Find("Anxious");
        Assert.NotNull(anxious);

        for (var i = 0; i < Iterations; i++)
        {
            var p = FaithLibrary.PickForEmotion(FaithTradition.Christianity, anxious);
            Assert.NotNull(p);
            Assert.StartsWith("chr-fear-", p!.Id);
        }
    }

    [Fact]
    public void PickForEmotion_SpecificFeeling_IncludesItsFocusedPool()
    {
        // "Lonely" (Sad core) has a focused loneliness pool plus the Sad core pool.
        var lonely = EmotionCatalog.Find("Lonely");
        Assert.NotNull(lonely);

        var allowed = new[] { "chr-lon-", "chr-sad-" };
        var sawLon = false;
        for (var i = 0; i < Iterations; i++)
        {
            var p = FaithLibrary.PickForEmotion(FaithTradition.Christianity, lonely);
            Assert.NotNull(p);
            Assert.True(allowed.Any(a => p!.Id.StartsWith(a)), $"Unexpected passage '{p!.Id}' for Lonely.");
            sawLon |= p.Id.StartsWith("chr-lon-");
        }
        Assert.True(sawLon, "Expected at least one loneliness-specific passage.");
    }

    [Fact]
    public void PickForEmotion_UnstockedTradition_ReturnsNull()
    {
        // An unknown/unstocked tradition value must fall through to null.
        var unknown = (FaithTradition)99;
        Assert.Null(FaithLibrary.PickForEmotion(unknown, null));
        Assert.Null(FaithLibrary.PickForEmotion(unknown, EmotionCatalog.Find("Happy")));
    }

    [Fact]
    public void PickForEmotion_Hinduism_ReturnsAGitaPassage()
    {
        // "Overwhelmed" is under the Bad core → the Hindu "bad" pool.
        var overwhelmed = EmotionCatalog.Find("Overwhelmed");
        for (var i = 0; i < Iterations; i++)
        {
            var p = FaithLibrary.PickForEmotion(FaithTradition.Hinduism, overwhelmed);
            Assert.NotNull(p);
            Assert.Equal(FaithTradition.Hinduism, p!.Tradition);
            Assert.StartsWith("hin-", p.Id);
        }
    }

    [Fact]
    public void PickForEmotion_Islam_ReturnsAnIslamicPassage()
    {
        // "Anxious" is under the Fearful core → the Islamic fear pool.
        var anxious = EmotionCatalog.Find("Anxious");
        for (var i = 0; i < Iterations; i++)
        {
            var p = FaithLibrary.PickForEmotion(FaithTradition.Islam, anxious);
            Assert.NotNull(p);
            Assert.Equal(FaithTradition.Islam, p!.Tradition);
            Assert.StartsWith("islam-", p.Id);
        }
    }

    [Fact]
    public void PickForEmotion_Judaism_NullEmotion_ReturnsGeneralPassage()
    {
        for (var i = 0; i < Iterations; i++)
        {
            var p = FaithLibrary.PickForEmotion(FaithTradition.Judaism, null);
            Assert.NotNull(p);
            Assert.Equal(FaithTradition.Judaism, p!.Tradition);
            Assert.StartsWith("jud-gen-", p.Id);
        }
    }

    // ---- Daily ----

    [Fact]
    public void Daily_NoneSelected_IsNull()
    {
        Assert.Null(FaithLibrary.Daily(Array.Empty<FaithTradition>(), new DateOnly(2026, 7, 3)));
    }

    [Fact]
    public void Daily_IsDeterministicPerDay()
    {
        var date = new DateOnly(2026, 7, 3);
        var selected = new[] { FaithTradition.Christianity };

        var a = FaithLibrary.Daily(selected, date);
        var b = FaithLibrary.Daily(selected, date);

        Assert.NotNull(a);
        Assert.Equal(a!.Id, b!.Id);
        Assert.StartsWith("chr-gen-", a.Id);
    }

    [Fact]
    public void Daily_SkipsUnstockedSelections()
    {
        // An unknown/unstocked tradition must be skipped; the stocked (Christian)
        // one is still returned rather than null.
        var p = FaithLibrary.Daily(new[] { (FaithTradition)99, FaithTradition.Christianity }, new DateOnly(2026, 7, 3));
        Assert.NotNull(p);
        Assert.Equal(FaithTradition.Christianity, p!.Tradition);
    }

    [Fact]
    public void Daily_RotatesAcrossSelectedTraditionsByDay()
    {
        var selected = new[] { FaithTradition.Christianity, FaithTradition.Islam };
        var d1 = new DateOnly(2026, 7, 3);

        var p1 = FaithLibrary.Daily(selected, d1);
        var p2 = FaithLibrary.Daily(selected, d1.AddDays(1));

        Assert.NotNull(p1);
        Assert.NotNull(p2);
        // Two stocked traditions → consecutive days alternate which one is shown.
        Assert.NotEqual(p1!.Tradition, p2!.Tradition);
    }
}
