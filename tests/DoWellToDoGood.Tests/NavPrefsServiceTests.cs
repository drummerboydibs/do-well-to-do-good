using DoWellToDoGood.Models;
using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

/// <summary>
/// Covers the pure reconciliation logic behind the navigation layout: catalog
/// reconcile (drop unknown, dedupe, append new pages), reordering with clamping,
/// auth/hidden-aware visibility, last-write-wins sync, and payload round-tripping.
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
        var result = NavPrefsService.ReconcileOrder(new[] { "resources", "resources", "nope", "write" });

        // Known, deduped, in the given order...
        Assert.Equal("resources", result[0]);
        Assert.Equal("write", result[1]);
        Assert.DoesNotContain("nope", result);
        Assert.Single(result, k => k == "resources");
    }

    [Fact]
    public void ReconcileOrder_AppendsCatalogPagesMissingFromSaved()
    {
        // Simulates an old saved layout from before some pages existed.
        var result = NavPrefsService.ReconcileOrder(new[] { "resources", "write" });

        Assert.Equal(Default.Count, result.Count);
        Assert.Equal(new[] { "resources", "write" }, result.Take(2));
        foreach (var key in Default) Assert.Contains(key, result); // nothing lost
    }

    [Fact]
    public void ReconcileHidden_KeepsKnownReorderableKeysOnly()
    {
        // "home" is a fixed anchor (not reorderable); "nope" is unknown — both dropped.
        var hidden = NavPrefsService.ReconcileHidden(new[] { "recovery", "home", "nope" });

        Assert.Contains("recovery", hidden);
        Assert.DoesNotContain("home", hidden);
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
    public void ComputeVisible_Guest_ExcludesAuthGatedPages()
    {
        var visible = NavPrefsService.ComputeVisible(Default, new HashSet<string>(), signedIn: false);

        Assert.All(visible, i => Assert.False(i.RequiresAuth));
        Assert.Contains(visible, i => i.Key == "write");
        Assert.Contains(visible, i => i.Key == "resources");
        Assert.DoesNotContain(visible, i => i.Key == "recovery");
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
        var hidden = new HashSet<string> { "write" };
        var visible = NavPrefsService.ComputeVisible(Default, hidden, signedIn: true);
        Assert.DoesNotContain(visible, i => i.Key == "write");
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
            new List<string> { "recovery", "write", "sleep" },
            new List<string> { "resources" },
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
