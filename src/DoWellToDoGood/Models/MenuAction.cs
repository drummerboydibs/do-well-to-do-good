namespace DoWellToDoGood.Models;

/// <summary>
/// One entry in a row's "More actions" sheet (<c>ActionMenu</c>).
/// </summary>
/// <param name="Text">Visible label, e.g. "Edit" or "Delete".</param>
/// <param name="Run">What selecting it does. Runs after the sheet has closed.</param>
/// <param name="Danger">Destructive — styled as a warning. Any confirm step stays in <paramref name="Run"/>.</param>
public sealed record MenuAction(string Text, Func<Task> Run, bool Danger = false)
{
    /// <summary>Convenience for synchronous actions (e.g. toggling edit mode).</summary>
    public MenuAction(string text, Action run, bool danger = false)
        : this(text, () => { run(); return Task.CompletedTask; }, danger) { }
}
