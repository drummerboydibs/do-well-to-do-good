using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class EmotionCatalogTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("definitely not a feeling")]
    public void Find_ReturnsNull_ForBlankOrUnknown(string? name)
    {
        Assert.Null(EmotionCatalog.Find(name));
    }

    [Fact]
    public void Find_Core_HasTier1_AndEmptyTrail()
    {
        var info = EmotionCatalog.Find("Happy");

        Assert.NotNull(info);
        Assert.Equal("Happy", info!.Name);
        Assert.Equal(1, info.Tier);
        Assert.Equal("", info.Trail);
        Assert.Equal("happy", info.CoreKey);
        Assert.Equal("Happy", info.CoreName);
        Assert.True(info.Valence > 0);
    }

    [Fact]
    public void Find_Secondary_HasTier2_AndCoreTrail()
    {
        var info = EmotionCatalog.Find("Lonely");

        Assert.NotNull(info);
        Assert.Equal(2, info!.Tier);
        Assert.Equal("Sad", info.Trail);
        Assert.Equal("sad", info.CoreKey);
    }

    [Fact]
    public void Find_Tertiary_HasTier3_AndBreadcrumbTrail()
    {
        var info = EmotionCatalog.Find("Grief");

        Assert.NotNull(info);
        Assert.Equal(3, info!.Tier);
        Assert.Equal("Sad › Despair", info.Trail);
        Assert.Equal("sad", info.CoreKey);
    }

    [Fact]
    public void Find_Tertiary_InheritsSecondaryValence()
    {
        // "Grief" is a tertiary under the secondary "Despair" (valence -0.85).
        var despair = EmotionCatalog.Find("Despair");
        var grief = EmotionCatalog.Find("Grief");

        Assert.NotNull(despair);
        Assert.NotNull(grief);
        Assert.Equal(despair!.Valence, grief!.Valence);
    }

    [Theory]
    [InlineData("happy")]
    [InlineData("HAPPY")]
    [InlineData("HaPpY")]
    public void Find_IsCaseInsensitive(string name)
    {
        var info = EmotionCatalog.Find(name);
        Assert.NotNull(info);
        Assert.Equal("Happy", info!.Name);
    }

    [Fact]
    public void AllNames_IsSorted_Distinct_AndNonEmpty()
    {
        var names = EmotionCatalog.AllNames().ToList();

        Assert.NotEmpty(names);

        // Distinct (case-insensitive).
        var distinct = names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(distinct.Count, names.Count);

        // Sorted ascending (case-insensitive).
        var sorted = names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, names);
    }

    [Fact]
    public void AllNames_IncludesEveryRing()
    {
        var names = EmotionCatalog.AllNames().ToList();

        Assert.Contains("Happy", names);     // core
        Assert.Contains("Lonely", names);    // secondary
        Assert.Contains("Grief", names);     // tertiary
    }

    [Fact]
    public void AllNames_AreAllResolvableByFind()
    {
        foreach (var name in EmotionCatalog.AllNames())
            Assert.NotNull(EmotionCatalog.Find(name));
    }

    // ---- GentleFeelings (the word-of-the-day pool) ----

    [Fact]
    public void GentleFeelings_IsNonEmpty_WithDefinitions()
    {
        var gentle = EmotionCatalog.GentleFeelings();

        Assert.NotEmpty(gentle);
        Assert.All(gentle, f =>
        {
            Assert.False(string.IsNullOrWhiteSpace(f.Name));
            Assert.False(string.IsNullOrWhiteSpace(f.Definition));
        });
    }

    [Fact]
    public void GentleFeelings_ExcludesEveryNegativeFeeling()
    {
        // The whole point: a random word of the day must never be a heavy feeling.
        // Every entry must resolve to a feeling whose valence is non-negative.
        foreach (var (name, _) in EmotionCatalog.GentleFeelings())
        {
            var info = EmotionCatalog.Find(name);
            Assert.NotNull(info);
            Assert.True(info!.Valence >= 0, $"'{name}' has negative valence ({info.Valence}) and shouldn't be a gentle feeling.");
        }
    }

    [Theory]
    [InlineData("Happy")]     // positive core
    [InlineData("Joyful")]    // positive tertiary
    [InlineData("Peaceful")]  // positive secondary
    public void GentleFeelings_IncludesPositiveFeelings(string name)
    {
        Assert.Contains(EmotionCatalog.GentleFeelings(), f => f.Name == name);
    }

    [Theory]
    [InlineData("Despair")]   // very negative secondary
    [InlineData("Grief")]     // negative tertiary
    [InlineData("Sad")]       // negative core
    [InlineData("Angry")]     // negative core
    [InlineData("Confused")]  // negative secondary under a positive core (Surprised)
    public void GentleFeelings_OmitsHeavyFeelings(string name)
    {
        Assert.DoesNotContain(EmotionCatalog.GentleFeelings(), f => f.Name == name);
    }
}
