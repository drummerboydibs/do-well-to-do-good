using Microsoft.JSInterop;

namespace DoWellToDoGood.Tests;

/// <summary>
/// A hand-rolled <see cref="IJSRuntime"/> for unit tests. It emulates the
/// browser's localStorage in a dictionary and lets a test supply a handler for
/// any other JS call. Every invocation is recorded for assertions.
/// </summary>
public sealed class FakeJsRuntime : IJSRuntime
{
    /// <summary>In-memory stand-in for window.localStorage.</summary>
    public Dictionary<string, string?> Store { get; } = new();

    /// <summary>Every (identifier, args) pair that was invoked, in order.</summary>
    public List<(string Identifier, object?[]? Args)> Calls { get; } = new();

    /// <summary>Handler for non-localStorage identifiers. Return the value the JS call should yield.</summary>
    public Func<string, object?[]?, object?>? Handler { get; set; }

    /// <summary>When true, every invocation throws — simulates JS not being ready.</summary>
    public bool ThrowOnEveryCall { get; set; }

    public int CallCountFor(string identifier) => Calls.Count(c => c.Identifier == identifier);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        Calls.Add((identifier, args));
        if (ThrowOnEveryCall) throw new InvalidOperationException("JS interop unavailable");

        object? result = Dispatch(identifier, args);
        return new ValueTask<TValue>((TValue)result!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);

    private object? Dispatch(string identifier, object?[]? args)
    {
        switch (identifier)
        {
            case "localStorage.getItem":
            {
                var key = (string)args![0]!;
                return Store.TryGetValue(key, out var v) ? v : null;
            }
            case "localStorage.setItem":
                Store[(string)args![0]!] = (string?)args[1];
                return null; // void call
            case "localStorage.removeItem":
                Store.Remove((string)args![0]!);
                return null; // void call
            default:
                return Handler?.Invoke(identifier, args);
        }
    }
}
