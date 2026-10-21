namespace DoWellToDoGood.Models;

/// <summary>
/// One destination in the app's navigation — either a fixed primary
/// destination (Today, Journal, Track, Get help, Account) or a tracker that
/// lives under Track (Sleep, Recovery, Therapy).
/// </summary>
/// <param name="Key">Stable id used in saved preferences (never change it).</param>
/// <param name="Href">Route, relative to base ("" is Today).</param>
/// <param name="Label">Full label (desktop row, cards, editor, tabs).</param>
/// <param name="ShortLabel">Terse label for the cramped bottom bar.</param>
/// <param name="Icon">Icon name understood by <c>NavIcon</c>.</param>
/// <param name="Blurb">One-line description for guest feature cards and onboarding.</param>
/// <param name="RequiresAuth">Only usable by signed-in users.</param>
public sealed record NavItem(
    string Key,
    string Href,
    string Label,
    string ShortLabel,
    string Icon,
    string Blurb,
    bool RequiresAuth = false);

public static class NavCatalog
{
    public static readonly NavItem Today = new("today", "", "Today", "Today", "home",
        "Your day at a glance — streaks, goals, and a thought to carry with you.");

    public static readonly NavItem Journal = new("journal", "journal", "Journal", "Journal", "book",
        "Write out what's on your mind. Save it privately, or shout it into the wind.");

    public static readonly NavItem Track = new("track", "track", "Track", "Track", "track",
        "Sleep, recovery, and therapy — only the ones you choose.");

    public static readonly NavItem Help = new("help", "help", "Get help", "Help", "heart",
        "Vetted guides and helplines when you need more support.");

    public static readonly NavItem Account = new("account", "account", "Account", "Account", "user",
        "Your account, appearance, and settings.");

    /// <summary>The fixed primary destinations, in display order. Never hidden or reordered.</summary>
    public static readonly IReadOnlyList<NavItem> Primary = new[] { Today, Journal, Track, Help, Account };

    /// <summary>
    /// The trackers under Track — the only part of navigation a user customises
    /// (which are on, and their order). Declaration order is the default order.
    /// Keys match the pre-"Track" catalog so saved layouts carry over.
    /// </summary>
    public static readonly IReadOnlyList<NavItem> Trackers = new[]
    {
        new NavItem("sleep", "track/sleep", "Sleep", "Sleep", "moon",
            "Log how you slept and spot what helps you rest.", RequiresAuth: true),

        new NavItem("recovery", "track/recovery", "Recovery", "Recovery", "sprout",
            "Count your sober days and celebrate milestones.", RequiresAuth: true),

        new NavItem("therapy", "track/therapy", "Therapy", "Therapy", "chat",
            "Log sessions and work toward the goals you set.", RequiresAuth: true),
    };

    /// <summary>Look up a tracker by key (trackers are the only keyed, customisable items).</summary>
    public static NavItem? ById(string key) => Trackers.FirstOrDefault(i => i.Key == key);

    /// <summary>The tracker keys, in default order.</summary>
    public static List<string> DefaultOrder() => Trackers.Select(i => i.Key).ToList();

    /// <summary>
    /// Whether <paramref name="currentPath"/> (base-relative, no query) belongs
    /// to the section at <paramref name="href"/>: an exact match, or a sub-route
    /// ("track" owns "track/sleep/review"). The root ("") only matches itself.
    /// </summary>
    public static bool IsCurrent(string currentPath, string href)
    {
        var path = StripQuery(currentPath).Trim('/');
        var target = href.Trim('/');
        if (target.Length == 0) return path.Length == 0;
        return string.Equals(path, target, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(target + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string StripQuery(string path)
    {
        var i = path.IndexOfAny(new[] { '?', '#' });
        return i < 0 ? path : path[..i];
    }
}
