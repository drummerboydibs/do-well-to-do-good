using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class ThemeServiceTests
{
    [Fact]
    public void DefaultsToSystem()
    {
        var svc = new ThemeService(new FakeJsRuntime());
        Assert.Equal(ThemeService.System, svc.Current);
    }

    [Theory]
    [InlineData(ThemeService.Light)]
    [InlineData(ThemeService.Dark)]
    [InlineData(ThemeService.System)]
    public async Task SetAsync_ValidPreference_UpdatesCurrent_RaisesChanged_AndPersists(string pref)
    {
        var js = new FakeJsRuntime();
        var svc = new ThemeService(js);
        var changed = 0;
        svc.Changed += () => changed++;

        await svc.SetAsync(pref);

        Assert.Equal(pref, svc.Current);
        Assert.Equal(1, changed);
        Assert.Equal(1, js.CallCountFor("dwtdgTheme.set"));
        Assert.Equal(pref, js.Calls.Single(c => c.Identifier == "dwtdgTheme.set").Args![0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("blue")]
    [InlineData("LIGHT")] // case-sensitive on purpose
    public async Task SetAsync_InvalidPreference_IsIgnored(string pref)
    {
        var js = new FakeJsRuntime();
        var svc = new ThemeService(js);
        var changed = 0;
        svc.Changed += () => changed++;

        await svc.SetAsync(pref);

        Assert.Equal(ThemeService.System, svc.Current);
        Assert.Equal(0, changed);
        Assert.Equal(0, js.CallCountFor("dwtdgTheme.set"));
    }

    [Fact]
    public async Task InitializeAsync_AdoptsStoredPreference()
    {
        var js = new FakeJsRuntime { Handler = (id, _) => id == "dwtdgTheme.get" ? ThemeService.Dark : null };
        var svc = new ThemeService(js);

        await svc.InitializeAsync();

        Assert.Equal(ThemeService.Dark, svc.Current);
    }

    [Fact]
    public async Task InitializeAsync_SwallowsJsErrors_AndKeepsDefault()
    {
        var js = new FakeJsRuntime { ThrowOnEveryCall = true };
        var svc = new ThemeService(js);

        await svc.InitializeAsync(); // must not throw

        Assert.Equal(ThemeService.System, svc.Current);
    }
}
