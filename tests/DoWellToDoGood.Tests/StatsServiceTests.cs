using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class StatsServiceTests
{
    private const string Key = "dwtdg.shouts";
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    [Fact]
    public async Task GetShoutsAsync_EmptyByDefault()
    {
        var svc = new StatsService(new FakeJsRuntime());

        var (count, days) = await svc.GetShoutsAsync();

        Assert.Equal(0, count);
        Assert.Empty(days);
    }

    [Fact]
    public async Task RecordShoutAsync_IncrementsCount_AndRecordsToday()
    {
        var svc = new StatsService(new FakeJsRuntime());

        await svc.RecordShoutAsync();
        var (count, days) = await svc.GetShoutsAsync();

        Assert.Equal(1, count);
        Assert.Equal(new[] { Today }, days);
    }

    [Fact]
    public async Task RecordShoutAsync_SameDayTwice_CountsTwice_ButOneDay()
    {
        var svc = new StatsService(new FakeJsRuntime());

        await svc.RecordShoutAsync();
        await svc.RecordShoutAsync();
        var (count, days) = await svc.GetShoutsAsync();

        Assert.Equal(2, count);                  // every shout counts
        Assert.Equal(new[] { Today }, days);     // ...but today is one calendar day
    }

    [Fact]
    public async Task RecordShoutAsync_PersistsAcrossServiceInstances()
    {
        var js = new FakeJsRuntime();

        await new StatsService(js).RecordShoutAsync();
        var (count, days) = await new StatsService(js).GetShoutsAsync(); // fresh instance, same storage

        Assert.Equal(1, count);
        Assert.Single(days);
    }

    [Fact]
    public async Task GetShoutsAsync_ToleratesCorruptStorage()
    {
        var js = new FakeJsRuntime();
        js.Store[Key] = "{ not valid json";
        var svc = new StatsService(js);

        var (count, days) = await svc.GetShoutsAsync();

        Assert.Equal(0, count);
        Assert.Empty(days);
    }

    [Fact]
    public async Task GetShoutsAsync_SkipsUnparseableDayStrings()
    {
        var js = new FakeJsRuntime();
        // One valid day, one garbage entry — only the valid one should survive parsing.
        var today = Today.ToString("yyyy-MM-dd");
        js.Store[Key] = $"{{\"count\":3,\"days\":[\"{today}\",\"not-a-date\"]}}";
        var svc = new StatsService(js);

        var (count, days) = await svc.GetShoutsAsync();

        Assert.Equal(3, count);
        Assert.Equal(new[] { Today }, days);
    }
}
