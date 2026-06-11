using Microsoft.JSInterop;

namespace DoWellToDoGood.Services;

/// <summary>
/// C# face of the in-browser envelope encryption (wwwroot/js/crypto.js).
/// The data-encryption key lives only in JS memory — nothing on the C# side
/// (or the server) can ever read it back out.
/// </summary>
public class CryptoService(IJSRuntime js)
{
    public bool IsUnlocked { get; private set; }
    public event Action? Changed;

    public record KeySetup(string KekSalt, string WrappedDek, string RecoverySalt, string RecoveryWrappedDek, string RecoveryCode);

    public async Task<KeySetup> SetupAsync(string passphrase)
    {
        var result = await js.InvokeAsync<KeySetup>("dwtdgCrypto.setup", passphrase);
        IsUnlocked = true;
        Changed?.Invoke();
        return result;
    }

    public async Task<bool> UnlockAsync(string passphrase, string kekSalt, string wrappedDek)
    {
        IsUnlocked = await js.InvokeAsync<bool>("dwtdgCrypto.unlock", passphrase, kekSalt, wrappedDek);
        if (IsUnlocked) Changed?.Invoke();
        return IsUnlocked;
    }

    public async Task<bool> UnlockWithRecoveryAsync(string code, string recoverySalt, string recoveryWrappedDek)
    {
        IsUnlocked = await js.InvokeAsync<bool>("dwtdgCrypto.unlockWithRecovery", code, recoverySalt, recoveryWrappedDek);
        if (IsUnlocked) Changed?.Invoke();
        return IsUnlocked;
    }

    public ValueTask<string> EncryptAsync(string plaintext) => js.InvokeAsync<string>("dwtdgCrypto.encryptText", plaintext);
    public ValueTask<string> DecryptAsync(string payload) => js.InvokeAsync<string>("dwtdgCrypto.decryptText", payload);

    public async Task LockAsync()
    {
        await js.InvokeVoidAsync("dwtdgCrypto.lock");
        IsUnlocked = false;
        Changed?.Invoke();
    }
}
