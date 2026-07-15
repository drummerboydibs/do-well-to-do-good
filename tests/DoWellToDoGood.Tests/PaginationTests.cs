using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class PaginationTests
{
    // ---- TotalPages ----

    [Theory]
    [InlineData(0, 10, 1)]    // empty list still reads as "page 1 of 1"
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]   // exactly one full page
    [InlineData(11, 10, 2)]   // one over spills to a second page
    [InlineData(20, 10, 2)]
    [InlineData(21, 10, 3)]
    [InlineData(95, 10, 10)]
    [InlineData(100, 10, 10)]
    public void TotalPages_RoundsUp(int totalItems, int pageSize, int expected)
    {
        Assert.Equal(expected, Pagination.TotalPages(totalItems, pageSize));
    }

    [Fact]
    public void TotalPages_NegativeTotal_IsOnePage()
    {
        Assert.Equal(1, Pagination.TotalPages(-5, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TotalPages_NonPositivePageSize_Throws(int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Pagination.TotalPages(10, pageSize));
    }

    // ---- ClampPage ----

    [Theory]
    [InlineData(2, 25, 10, 2)]     // in range, unchanged (3 pages)
    [InlineData(1, 25, 10, 1)]
    [InlineData(3, 25, 10, 3)]
    [InlineData(0, 25, 10, 1)]     // below floor → first page
    [InlineData(-7, 25, 10, 1)]
    [InlineData(99, 25, 10, 3)]    // above ceiling → last page
    [InlineData(1, 0, 10, 1)]      // empty list → page 1
    public void ClampPage_KeepsPageInRange(int page, int totalItems, int pageSize, int expected)
    {
        Assert.Equal(expected, Pagination.ClampPage(page, totalItems, pageSize));
    }

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
        Assert.Equal(expected, Pagination.ParseContentRangeTotal(header));
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
        Assert.Null(Pagination.ParseContentRangeTotal(header));
    }

    // ---- TotalFromResponse ----
    // PostgREST returns Content-Range on the content headers. (TotalFromResponse
    // also checks the response headers, but .NET classifies Content-Range as a
    // content header and refuses to put it anywhere else, so that fallback can't
    // be reached through a normal HttpClient — it's inherited belt-and-braces.)

    private static HttpResponseMessage ResponseWith(string? contentRange)
    {
        var res = new HttpResponseMessage { Content = new StringContent("[]") };
        if (contentRange is not null)
            res.Content.Headers.TryAddWithoutValidation("Content-Range", contentRange);
        return res;
    }

    [Fact]
    public void TotalFromResponse_ReadsContentRangeFromContentHeaders()
    {
        using var res = ResponseWith("0-9/57");
        Assert.Equal(57, Pagination.TotalFromResponse(res, fallback: 10));
    }

    [Theory]
    [InlineData(null)]      // header absent entirely
    [InlineData("0-9/*")]   // total unknown
    [InlineData("garbage")]
    public void TotalFromResponse_MissingOrUnparseable_UsesFallback(string? contentRange)
    {
        using var res = ResponseWith(contentRange);
        Assert.Equal(10, Pagination.TotalFromResponse(res, fallback: 10));
    }
}
