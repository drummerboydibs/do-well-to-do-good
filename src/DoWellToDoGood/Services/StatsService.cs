using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace DoWellToDoGood.Services;

/// <summary>
/// Tracks "shout into the wind" activity for the account summary. Honouring the
/// app's privacy promise, this stores NO content — only a running count and the
/// set of local calendar days a shout happened on, in localStorage, on this
/// device. Saved-entry stats come from the server instead (timestamps only).
/// </summary>
public class StatsService(IJSRuntime js)
{
    private const string Key = "dwtdg.shouts";
    private const int DayCap = 730; // ~2 years of distinct days is ample for any streak

    private record ShoutData(
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("days")] List<string> Days);

    public async Task<(int Count, IReadOnlyList<DateOnly> Days)> GetShoutsAsync()
    {
        var data = await LoadAsync();
        return (data.Count, ParseDays(data.Days));
    }

    public async Task RecordShoutAsync()
    {
        try
        {
            var data = await LoadAsync();
            var today = Today().ToString("yyyy-MM-dd");
            var days = data.Days;
            if (days.Count == 0 || days[^1] != today)
            {
                days.Add(today);
                if (days.Count > DayCap) days.RemoveRange(0, days.Count - DayCap);
            }
            var next = new ShoutData(data.Count + 1, days);
            await js.InvokeVoidAsync("localStorage.setItem", Key, JsonSerializer.Serialize(next));
        }
        catch
        {
            // Stats are a nicety — never let them break the journaling flow.
        }
    }

    private async Task<ShoutData> LoadAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            if (json is null) return new ShoutData(0, new());
            var data = JsonSerializer.Deserialize<ShoutData>(json);
            return data is null ? new ShoutData(0, new()) : data with { Days = data.Days ?? new() };
        }
        catch
        {
            return new ShoutData(0, new());
        }
    }

    private static List<DateOnly> ParseDays(IEnumerable<string> days)
    {
        var list = new List<DateOnly>();
        foreach (var d in days)
            if (DateOnly.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                list.Add(parsed);
        return list;
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// Current run of consecutive local days with at least one logged activity,
    /// ending today (or yesterday, so the streak survives until the day is out).
    /// </summary>
    public static int CurrentStreak(IEnumerable<DateOnly> activeDays)
    {
        var set = activeDays.ToHashSet();
        if (set.Count == 0) return 0;

        var today = Today();
        DateOnly cursor;
        if (set.Contains(today)) cursor = today;
        else if (set.Contains(today.AddDays(-1))) cursor = today.AddDays(-1);
        else return 0;

        var streak = 0;
        while (set.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }
}
