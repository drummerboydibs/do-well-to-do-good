using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class EntriesServiceTests
{
    // ---- ParseContentRangeTotal ----
    // PostgREST returns the grand total after the slash, e.g. "0-9/57".

    [Theory]
    [InlineData("0-9/57", 57)]
    [InlineData("0-0/1", 1)]
    [InlineData("*/0", 0)]            // empty result set
    [InlineData("0-24/100", 100)]
    [InlineData(" 0-9/57 ", 57)]      // surrounding whitespace tolerated
    public void ParseContentRangeTotal_ReadsGrandTotal(string header, int expected)
    {
        Assert.Equal(expected, EntriesService.ParseContentRangeTotal(header));
    }

    [Theory]
    [InlineData("0-9/*")]             // total unknown (count not requested)
    [InlineData("0-9")]               // no slash at all
    [InlineData("garbage")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ParseContentRangeTotal_MissingOrUnparseable_IsNull(string? header)
    {
        Assert.Null(EntriesService.ParseContentRangeTotal(header));
    }
}
