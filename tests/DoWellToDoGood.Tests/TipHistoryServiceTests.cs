using System.Text.Json;
using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class TipHistoryServiceTests
{
    private const string Key = "dwtdg.tips.recent";

    [Fact]
    public async Task GetRecentAsync_EmptyByDefault()
    {
        var svc = new TipHistoryService(new FakeJsRuntime());
        Assert.Empty(await svc.GetRecentAsync());
    }

    [Fact]
    public async Task RecordAsync_AppendsId()
    {
        var js = new FakeJsRuntime();
        var svc = new TipHistoryService(js);

        await svc.RecordAsync("hap-1");

        Assert.Equal(new[] { "hap-1" }, await svc.GetRecentAsync());
    }

    [Fact]
    public async Task RecordAsync_MovesRepeatToTheEnd_WithoutDuplicating()
    {
        var js = new FakeJsRuntime();
        var svc = new TipHistoryService(js);

        await svc.RecordAsync("a");
        await svc.RecordAsync("b");
        await svc.RecordAsync("a"); // seen again

        Assert.Equal(new[] { "b", "a" }, await svc.GetRecentAsync());
    }

    [Fact]
    public async Task RecordAsync_CapsHistoryAt24_KeepingMostRecent()
    {
        var js = new FakeJsRuntime();
        var svc = new TipHistoryService(js);

        for (var i = 0; i < 30; i++)
            await svc.RecordAsync($"tip-{i}");

        var recent = (await svc.GetRecentAsync()).ToList();

        Assert.Equal(24, recent.Count);
        Assert.Equal("tip-29", recent[^1]);          // newest kept
        Assert.Equal("tip-6", recent[0]);            // oldest survivor (30 - 24)
        Assert.DoesNotContain("tip-5", recent);      // evicted
    }

    [Fact]
    public async Task GetRecentAsync_ToleratesCorruptStorage()
    {
        var js = new FakeJsRuntime();
        js.Store[Key] = "this is not json";
        var svc = new TipHistoryService(js);

        Assert.Empty(await svc.GetRecentAsync());
    }

    [Fact]
    public async Task RecordAsync_PersistsAsJsonArray()
    {
        var js = new FakeJsRuntime();
        var svc = new TipHistoryService(js);

        await svc.RecordAsync("x");

        var stored = JsonSerializer.Deserialize<string[]>(js.Store[Key]!);
        Assert.Equal(new[] { "x" }, stored);
    }
}
