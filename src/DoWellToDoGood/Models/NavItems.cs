namespace DoWellToDoGood.Models;

/// <summary>
/// One destination in the app's navigation. A single catalog (below) is the
/// source of truth for the desktop nav row, the mobile bottom bar, the Home
/// "explore" cards, and the Account customiser — so adding a page in one place
/// lights it up everywhere.
/// </summary>
/// <param name="Key">Stable id used in saved preferences (never change it).</param>
/// <param name="Href">Route, relative to base ("" is Home).</param>
/// <param name="Label">Full label (desktop row, cards, editor).</param>
/// <param name="ShortLabel">Terse label for the cramped bottom bar.</param>
/// <param name="Icon">Icon name understood by <c>NavIcon</c>.</param>
/// <param name="Blurb">One-line description for the Home explore cards.</param>
/// <param name="RequiresAuth">Only shown to signed-in users.</param>
/// <param name="Fixed">Structural anchor (Home, Account) — always shown, can't be hidden or reordered.</param>
public sealed record NavItem(
    string Key,
    string Href,
    string Label,
    string ShortLabel,
    string Icon,
    string Blurb,
    bool RequiresAuth = false,
    bool Fixed = false);

public static class NavCatalog
{
    /// <summary>Declaration order is the default nav order.</summary>
    public static readonly IReadOnlyList<NavItem> All = new[]
    {
        new NavItem("home", "", "Home", "Home", "home",
            "Your calm place to start — a snapshot of your journey.", Fixed: true),

        new NavItem("write", "journal", "Write", "Write", "pencil",
            "Write out what's on your mind. Save it, or shout it into the wind."),

        new NavItem("entries", "entries", "My journal", "Journal", "book",
            "Read back through the entries you've saved.", RequiresAuth: true),

        new NavItem("sleep", "sleep", "Sleep", "Sleep", "moon",
            "Log how you slept and spot what helps you rest.", RequiresAuth: true),

        new NavItem("therapy", "therapy", "Therapy", "Therapy", "chat",
            "Tools and prompts to get more from your therapy.", RequiresAuth: true),

        new NavItem("recovery", "recovery", "Recovery", "Recovery", "sprout",
            "Track your sober days, goals, and milestones.", RequiresAuth: true),

        new NavItem("resources", "resources", "Resources", "Guides", "compass",
            "Vetted guides and helplines when you need more support."),

        new NavItem("account", "signin", "Account", "Account", "user",
            "Your account, appearance, and settings.", Fixed: true),
    };

    public static NavItem? ById(string key) => All.FirstOrDefault(i => i.Key == key);

    /// <summary>The reorderable (non-fixed) keys, in default order.</summary>
    public static List<string> DefaultOrder() =>
        All.Where(i => !i.Fixed).Select(i => i.Key).ToList();
}
