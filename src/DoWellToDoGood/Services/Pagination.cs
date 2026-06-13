namespace DoWellToDoGood.Services;

/// <summary>
/// Pure helpers for splitting a list of items into fixed-size pages. Kept free
/// of any UI so the arithmetic (which is easy to get subtly wrong at the
/// boundaries) can be unit-tested on its own.
/// </summary>
public static class Pagination
{
    /// <summary>
    /// How many pages it takes to show <paramref name="totalItems"/> at
    /// <paramref name="pageSize"/> per page. Always at least 1 — an empty list
    /// still reads as "page 1 of 1" rather than "of 0".
    /// </summary>
    public static int TotalPages(int totalItems, int pageSize)
    {
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
        return totalItems <= 0 ? 1 : (totalItems + pageSize - 1) / pageSize;
    }

    /// <summary>Clamp a 1-based page number into the valid range [1, TotalPages].</summary>
    public static int ClampPage(int page, int totalItems, int pageSize) =>
        Math.Clamp(page, 1, TotalPages(totalItems, pageSize));
}
