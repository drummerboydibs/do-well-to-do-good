using DoWellToDoGood.Models;

namespace DoWellToDoGood.Tests;

public class WellnessTermsTests
{
    [Fact]
    public void All_IsNonEmpty()
    {
        Assert.NotEmpty(WellnessTerms.All);
    }

    [Fact]
    public void EveryTerm_HasANameAndDefinition()
    {
        Assert.All(WellnessTerms.All, term =>
        {
            Assert.False(string.IsNullOrWhiteSpace(term.Name));
            Assert.False(string.IsNullOrWhiteSpace(term.Definition));
        });
    }

    [Fact]
    public void TermNames_AreDistinct_IgnoringCase()
    {
        var dupes = WellnessTerms.All
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.True(dupes.Count == 0, $"Duplicate wellness terms: {string.Join(", ", dupes)}");
    }
}
