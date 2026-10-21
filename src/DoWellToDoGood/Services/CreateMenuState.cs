namespace DoWellToDoGood.Services;

/// <summary>
/// Open/closed state for the app-wide create menu. The triggers live inside
/// the header and bottom bar, whose backdrop blur makes them the containing
/// block for fixed-position children, so the sheet itself is rendered once at
/// the layout root (<c>CreateSheet</c>) and the two talk through this.
/// </summary>
public sealed class CreateMenuState
{
    private Func<ValueTask>? _refocus;

    public bool IsOpen { get; private set; }

    public event Action? Changed;

    /// <summary>Open the menu; <paramref name="refocus"/> returns focus to the trigger on dismiss.</summary>
    public void Open(Func<ValueTask> refocus)
    {
        _refocus = refocus;
        IsOpen = true;
        Changed?.Invoke();
    }

    /// <summary>
    /// Close the menu. Dismissing (Escape / scrim) restores focus to the
    /// trigger; choosing an item doesn't, because the new page takes focus.
    /// </summary>
    public async Task CloseAsync(bool restoreFocus)
    {
        if (!IsOpen) return;
        IsOpen = false;
        Changed?.Invoke();
        var refocus = _refocus;
        _refocus = null;
        if (restoreFocus && refocus is not null)
        {
            try { await refocus(); } catch { /* trigger hidden at this breakpoint */ }
        }
    }
}
