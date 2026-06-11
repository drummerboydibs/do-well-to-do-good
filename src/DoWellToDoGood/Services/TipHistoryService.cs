using System.Text.Json;
using Microsoft.JSInterop;

namespace DoWellToDoGood.Services;

/// <summary>
/// Remembers which tips this device has seen recently (tip IDs only, in
/// localStorage) so users don't get the same advice twice in a row. Stores no
/// journal content and nothing leaves the device.
/// </summary>
public class TipHistoryService(IJSRuntime js)
{
    private const string Key = "dwtdg.tips.recent";
    private const int Cap = 24;

    public async Task<IReadOnlyCollection<string>> GetRecentAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            return json is null
                ? Array.Empty<string>()
                : JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public async Task RecordAsync(string tipId)
    {
        try
        {
            var list = (await GetRecentAsync()).Where(id => id != tipId).ToList();
            list.Add(tipId);
            if (list.Count > Cap) list.RemoveRange(0, list.Count - Cap);
            await js.InvokeVoidAsync("localStorage.setItem", Key, JsonSerializer.Serialize(list));
        }
        catch
        {
            // History is a nicety — never let it break the journaling flow.
        }
    }
}
