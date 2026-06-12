using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class CryptoServiceTests
{
    private static readonly CryptoService.KeySetup SampleSetup =
        new("kek-salt", "wrapped-dek", "rec-salt", "rec-wrapped", "ABCD-EFGH");

    [Fact]
    public void StartsLocked()
    {
        Assert.False(new CryptoService(new FakeJsRuntime()).IsUnlocked);
    }

    [Fact]
    public async Task SetupAsync_Unlocks_AndRaisesChanged()
    {
        var js = new FakeJsRuntime { Handler = (id, _) => id == "dwtdgCrypto.setup" ? SampleSetup : null };
        var svc = new CryptoService(js);
        var changed = 0;
        svc.Changed += () => changed++;

        var result = await svc.SetupAsync("my passphrase");

        Assert.True(svc.IsUnlocked);
        Assert.Equal(1, changed);
        Assert.Equal(SampleSetup, result);
    }

    [Fact]
    public async Task UnlockAsync_Success_Unlocks_AndRaisesChanged()
    {
        var js = new FakeJsRuntime { Handler = (id, _) => id == "dwtdgCrypto.unlock" ? true : null };
        var svc = new CryptoService(js);
        var changed = 0;
        svc.Changed += () => changed++;

        var ok = await svc.UnlockAsync("pass", "salt", "wrapped");

        Assert.True(ok);
        Assert.True(svc.IsUnlocked);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task UnlockAsync_WrongPassphrase_StaysLocked_AndNoChanged()
    {
        var js = new FakeJsRuntime { Handler = (id, _) => id == "dwtdgCrypto.unlock" ? false : null };
        var svc = new CryptoService(js);
        var changed = 0;
        svc.Changed += () => changed++;

        var ok = await svc.UnlockAsync("wrong", "salt", "wrapped");

        Assert.False(ok);
        Assert.False(svc.IsUnlocked);
        Assert.Equal(0, changed);
    }

    [Fact]
    public async Task UnlockWithRecoveryAsync_Success_Unlocks()
    {
        var js = new FakeJsRuntime { Handler = (id, _) => id == "dwtdgCrypto.unlockWithRecovery" ? true : null };
        var svc = new CryptoService(js);

        var ok = await svc.UnlockWithRecoveryAsync("CODE", "rsalt", "rwrapped");

        Assert.True(ok);
        Assert.True(svc.IsUnlocked);
    }

    [Fact]
    public async Task LockAsync_RelocksAndRaisesChanged()
    {
        var js = new FakeJsRuntime
        {
            Handler = (id, _) => id == "dwtdgCrypto.unlock" ? true : null
        };
        var svc = new CryptoService(js);
        await svc.UnlockAsync("pass", "salt", "wrapped");
        Assert.True(svc.IsUnlocked);

        var changed = 0;
        svc.Changed += () => changed++;

        await svc.LockAsync();

        Assert.False(svc.IsUnlocked);
        Assert.Equal(1, changed);
    }
}
