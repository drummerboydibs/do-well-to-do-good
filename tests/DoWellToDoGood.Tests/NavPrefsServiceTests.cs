using DoWellToDoGood.Models;
using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

/// <summary>
/// Covers the pure reconciliation logic behind the tracker layout: catalog
/// reconcile (drop unknown, dedupe, append new trackers, migrate pre-Track
/// layouts), reordering with clamping, auth/hidden-aware visibility, the
/// first-run tracker choice, last-write-wins sync, and payload round-tripping.
/// </summary>
public class NavPrefsServiceTests
{
    private static readonly List<string> Default = NavCatalog.DefaultOrder();

    // ---- ReconcileOrder ----

    [Fact]
    public void ReconcileOrder_NullOrEmpty_ReturnsCatalogDefault()
    {
        Assert.Equal(Default, NavPrefsService.ReconcileOrder(null));
        Assert.Equal(Default, NavPrefsService.ReconcileOrder(Array.Empty<string>()));
    }

    [Fact]
    public void ReconcileOrder_DropsUnknownKeys_AndDedupes()
    {
        var result = NavPrefsService.ReconcileOrder(new[] { "therapy", "therapy", "nope", "sleep" });

        // Known, deduped, in the given order...
        Assert.Equal("therapy", result[0]);
        Assert.Equal("sleep", result[1]);
        Assert.DoesNotContain("nope", result);
        Assert.Single(result, k => k == "therapy");
    }

    [Fact]
    public void ReconcileOrder_AppendsTrackersMissingFromSaved()
    {
        // Simulates a saved layout from before a tracker existed.
        var result = NavPrefsService.ReconcileOrder(new[] { "therapy" });

        Assert.Equal(Default.Count, result.Count);
        Assert.Equal("therapy", result[0]);
        foreach (var key in Default) Assert.Contains(key, result); // nothing lost
    }

    [Fact]
    public void Reconcile_MigratesPreTrackLayout_KeepingTrackerOrderAndHidden()
    {
        // A layout saved when every page was in the nav: non-tracker pages drop
        // out; the trackers keep their relative order and hidden state.
        var order = NavPrefsService.ReconcileOrder(new[] { "write", "therapy", "resources", "sleep", "entries", "recovery" });
        var hidden = NavPrefsService.ReconcileHidden(new[] { "recovery", "write" });

        Assert.Equal(new[] { "therapy", "sleep", "recovery" }, order);
        Assert.Equal(new HashSet<string> { "recovery" }, hidden);
    }

    [Fact]
    public void ReconcileHidden_KeepsKnownTrackerKeysOnly()
    {
        // "today" is a fixed destination (not a tracker); "nope" is unknown — both dropped.
        var hidden = NavPrefsService.ReconcileHidden(new[] { "recovery", "today", "nope" });

        Assert.Contains("recovery", hidden);
        Assert.DoesNotContain("today", hidden);
        Assert.DoesNotContain("nope", hidden);
    }

    // ---- ApplyMove ----

    [Fact]
    public void ApplyMove_MovesItemAndLeavesOthersInOrder()
    {
        var moved = NavPrefsService.ApplyMove(Default, "recovery", -1);
        var i = Default.IndexOf("recovery");

        Assert.Equal("recovery", moved[i - 1]);
        Assert.Equal(Default.Count, moved.Count);
        Assert.Equal(Default.OrderBy(x => x), moved.OrderBy(x => x)); // same set
    }

    [Fact]
    public void ApplyMove_ClampsAtEnds()
    {
        Assert.Equal(Default, NavPrefsService.ApplyMove(Default, Default[0], -1));                  // already first
        Assert.Equal(Default, NavPrefsService.ApplyMove(Default, Default[^1], +1));                 // already last
        Assert.Equal(Default, NavPrefsService.ApplyMove(Default, Default[0], -99));                 // over-clamp
    }

    [Fact]
    public void ApplyMove_UnknownKey_ReturnsUnchanged()
    {
        Assert.Equal(Default, NavPrefsService.ApplyMove(Default, "nope", -1));
    }

    [Fact]
    public void ApplyMove_DoesNotMutateInput()
    {
        var input = NavCatalog.DefaultOrder();
        var snapshot = input.ToList();
        NavPrefsService.ApplyMove(input, "recovery", -2);
        Assert.Equal(snapshot, input);
    }

    // ---- ComputeVisible ----

    [Fact]
    public void ComputeVisible_Guest_SeesNoTrackers()
    {
        // Every tracker needs an account (its data is encrypted per user).
        var visible = NavPrefsService.ComputeVisible(Default, new HashSet<string>(), signedIn: false);
        Assert.Empty(visible);
    }

    [Fact]
    public void ComputeVisible_SignedIn_IncludesAuthGatedPagesInOrder()
    {
        var visible = NavPrefsService.ComputeVisible(Default, new HashSet<string>(), signedIn: true);
        Assert.Equal(Default, visible.Select(i => i.Key).ToList());
    }

    [Fact]
    public void ComputeVisible_ExcludesHidden()
    {
        var hidden = new HashSet<string> { "sleep" };
        var visible = NavPrefsService.ComputeVisible(Default, hidden, signedIn: true);
        Assert.DoesNotContain(visible, i => i.Key == "sleep");
        Assert.Equal(Default.Count - 1, visible.Count);
    }

    // ---- HiddenFor (first-run tracker choice) ----

    [Fact]
    public void HiddenFor_HidesEverythingNotChosen()
    {
        Assert.Equal(new HashSet<string> { "recovery" },
            NavPrefsService.HiddenFor(new[] { "sleep", "therapy" }));
    }

    [Fact]
    public void HiddenFor_AllChosen_HidesNothing()
    {
        Assert.Empty(NavPrefsService.HiddenFor(Default));
    }

    [Fact]
    public void HiddenFor_NoneChosen_HidesEveryTracker_IgnoringUnknownKeys()
    {
        Assert.Equal(Default.ToHashSet(), NavPrefsService.HiddenFor(new[] { "nope" }));
    }

    // ---- IsCurrent (section highlighting) ----

    [Theory]
    [InlineData("", "", true)]
    [InlineData("journal", "", false)]
    [InlineData("track/sleep/review", "track", true)]
    [InlineData("track", "track", true)]
    [InlineData("tracking", "track", false)]
    [InlineData("journal/new", "journal", true)]
    [InlineData("track/therapy?log=1", "track/therapy", true)]
    [InlineData("Track/Sleep", "track/sleep", true)]
    public void IsCurrent_MatchesSectionAndSubRoutes(string path, string href, bool expected)
    {
        Assert.Equal(expected, NavCatalog.IsCurrent(path, href));
    }

    // ---- DecideSync (last-write-wins) ----

    [Fact]
    public void DecideSync_ServerMissing_NoLocalEdits_DoesNothing()
    {
        Assert.Equal(NavPrefsService.SyncAction.None,
            NavPrefsService.DecideSync(DateTimeOffset.MinValue, server: null));
    }

    [Fact]
    public void DecideSync_ServerMissing_WithLocalEdits_PushesLocal()
    {
        Assert.Equal(NavPrefsService.SyncAction.PushLocal,
            NavPrefsService.DecideSync(DateTimeOffset.UtcNow, server: null));
    }

    [Fact]
    public void DecideSync_ServerNewer_AdoptsServer()
    {
        var local = DateTimeOffset.UtcNow;
        var server = local.AddMinutes(5);
        Assert.Equal(NavPrefsService.SyncAction.AdoptServer, NavPrefsService.DecideSync(local, server));
    }

    [Fact]
    public void DecideSync_LocalNewer_PushesLocal()
    {
        var server = DateTimeOffset.UtcNow;
        var local = server.AddMinutes(5);
        Assert.Equal(NavPrefsService.SyncAction.PushLocal, NavPrefsService.DecideSync(local, server));
    }

    [Fact]
    public void DecideSync_EqualTimestamps_DoesNothing()
    {
        var t = DateTimeOffset.UtcNow;
        Assert.Equal(NavPrefsService.SyncAction.None, NavPrefsService.DecideSync(t, t));
    }

    // ---- Wire round-trip ----

    [Fact]
    public void Serialize_Deserialize_RoundTripsLayout()
    {
        var when = DateTimeOffset.UtcNow;
        var wire = new NavPrefsService.Wire(
            new List<string> { "recovery", "therapy", "sleep" },
            new List<string> { "therapy" },
            when);

        var back = NavPrefsService.Deserialize(NavPrefsService.Serialize(wire));

        Assert.NotNull(back);
        Assert.Equal(wire.Order, back!.Order);
        Assert.Equal(wire.Hidden, back.Hidden);
        Assert.Equal(when, back.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{not json")]
    public void Deserialize_NullOrGarbage_ReturnsNull(string? raw)
    {
        Assert.Null(NavPrefsService.Deserialize(raw));
    }
}
