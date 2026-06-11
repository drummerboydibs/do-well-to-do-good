using Microsoft.JSInterop;

namespace DoWellToDoGood.Services;

/// <summary>
/// Holds the user's colour-theme preference (light / dark / system). The
/// heavy lifting — resolving "system" to a concrete theme, persisting it,
/// and reacting to OS changes — lives in wwwroot/js/theme.js; this is a thin
/// state-holding wrapper so Razor components can read and set it.
/// </summary>
public class ThemeService(IJSRuntime js)
{
    public const string System = "system";
    public const string Light = "light";
    public const string Dark = "dark";

    /// <summary>The stored preference, not the resolved theme ("system" stays "system").</summary>
    public string Current { get; private set; } = System;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        try { Current = await js.InvokeAsync<string>("dwtdgTheme.get") ?? System; }
        catch { /* JS not ready / no storage — keep the light-leaning default */ }
    }

    public async Task SetAsync(string preference)
    {
        if (preference != Light && preference != Dark && preference != System) return;
        Current = preference;
        try { await js.InvokeVoidAsync("dwtdgTheme.set", preference); }
        catch { /* applying is best-effort; the in-memory choice still updates the UI */ }
        Changed?.Invoke();
    }
}
