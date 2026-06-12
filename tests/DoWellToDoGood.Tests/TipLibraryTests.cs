using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class TipLibraryTests
{
    // Pick is randomized; run enough iterations that "could happen" becomes "did happen".
    private const int Iterations = 200;

    [Fact]
    public void Pick_NullEmotion_AlwaysReturnsFallbackTip()
    {
        for (var i = 0; i < Iterations; i++)
        {
            var tip = TipLibrary.Pick(null);
            Assert.StartsWith("gen-", tip.Id);
            Assert.False(string.IsNullOrWhiteSpace(tip.Text));
            Assert.False(string.IsNullOrWhiteSpace(tip.SourceUrl));
        }
    }

    [Fact]
    public void Pick_SpecificFeeling_DrawsFromItsFocusedPool()
    {
        // "anxious" maps to the Anxiety pool (ids "anx-*"), cascading into the
        // "fearful" core pool ("fea-*"). It must never wander outside those.
        var allowed = new[] { "anx-", "fea-" };
        var anxious = EmotionCatalog.Find("Anxious");
        Assert.NotNull(anxious);

        var seenAnx = false;
        for (var i = 0; i < Iterations; i++)
        {
            var tip = TipLibrary.Pick(anxious);
            Assert.True(allowed.Any(p => tip.Id.StartsWith(p)),
                $"Unexpected tip '{tip.Id}' for an anxious feeling.");
            seenAnx |= tip.Id.StartsWith("anx-");
        }

        Assert.True(seenAnx, "Expected at least one tip from the focused Anxiety pool.");
    }

    [Fact]
    public void Pick_CoreFeeling_DrawsFromCorePool()
    {
        var happy = EmotionCatalog.Find("Happy");
        Assert.NotNull(happy);

        for (var i = 0; i < Iterations; i++)
        {
            var tip = TipLibrary.Pick(happy);
            Assert.StartsWith("hap-", tip.Id);
        }
    }

    [Fact]
    public void Pick_AvoidsRecentIds_WhenFreshAlternativesExist()
    {
        var happy = EmotionCatalog.Find("Happy");
        // Exclude all but one of the Happy pool; the survivor must always be chosen.
        var recent = new[] { "hap-1", "hap-2", "hap-3" };

        for (var i = 0; i < Iterations; i++)
        {
            var tip = TipLibrary.Pick(happy, recent);
            Assert.DoesNotContain(tip.Id, recent);
        }
    }

    [Fact]
    public void Pick_FallsBackToFullPool_WhenEverythingIsRecent()
    {
        var happy = EmotionCatalog.Find("Happy");
        var allHappy = new[] { "hap-1", "hap-2", "hap-3", "hap-4" };

        // No fresh candidates remain, so it should still return a valid Happy tip
        // rather than throwing or returning a default.
        for (var i = 0; i < Iterations; i++)
        {
            var tip = TipLibrary.Pick(happy, allHappy);
            Assert.Contains(tip.Id, allHappy);
        }
    }

    [Fact]
    public void Pick_TertiaryFeeling_PullsFromParentPoolToo()
    {
        // "Overwhelmed" (tertiary under Bad › Stressed) maps to the Stress pool and,
        // because its parent "Stressed" also maps to Stress, plus the "bad" core.
        var overwhelmed = EmotionCatalog.Find("Overwhelmed");
        Assert.NotNull(overwhelmed);
        Assert.Equal(3, overwhelmed!.Tier);

        var allowed = new[] { "str-", "bad-" };
        for (var i = 0; i < Iterations; i++)
        {
            var tip = TipLibrary.Pick(overwhelmed);
            Assert.True(allowed.Any(p => tip.Id.StartsWith(p)),
                $"Unexpected tip '{tip.Id}' for 'Overwhelmed'.");
        }
    }
}
