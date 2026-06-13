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
}
