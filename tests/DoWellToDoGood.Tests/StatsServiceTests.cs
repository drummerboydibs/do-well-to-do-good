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
    public async Task RecordShoutAsync_NewDay_AppendsDay_AndKeepsRunningCount()
    {
        var js = new FakeJsRuntime();
        // A prior shout on an earlier day, count already at 5.
        js.Store[Key] = "{\"count\":5,\"days\":[\"2020-01-01\"]}";
        var svc = new StatsService(js);

        await svc.RecordShoutAsync(); // happens "today", a different calendar day

        var (count, days) = await svc.GetShoutsAsync();
        Assert.Equal(6, count);                                   // count carries forward
        Assert.Equal(2, days.Count);                             // old day + today
        Assert.Contains(new DateOnly(2020, 1, 1), days);
        Assert.Contains(Today, days);
    }

    [Fact]
    public async Task RecordShoutAsync_CapsDistinctDays_DroppingOldest()
    {
        var js = new FakeJsRuntime();
        // Seed 730 distinct past days (the cap) so today's shout overflows it.
        var start = new DateOnly(2018, 1, 1);
        var seeded = Enumerable.Range(0, 730).Select(i => start.AddDays(i).ToString("yyyy-MM-dd"));
        js.Store[Key] = $"{{\"count\":730,\"days\":[{string.Join(",", seeded.Select(d => $"\"{d}\""))}]}}";
        var svc = new StatsService(js);

        await svc.RecordShoutAsync();

        var (count, days) = await svc.GetShoutsAsync();
        Assert.Equal(731, count);                                 // every shout still counts
        Assert.Equal(730, days.Count);                           // ...but distinct days stay capped
        Assert.DoesNotContain(start, days);                      // oldest day evicted
        Assert.Contains(Today, days);                            // newest day kept
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
