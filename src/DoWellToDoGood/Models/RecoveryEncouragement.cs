namespace DoWellToDoGood.Models;

/// <summary>A short, non-judgmental message shown after a reset, with its source.</summary>
public record Encouragement(string Text, string SourceName, string SourceUrl);

/// <summary>
/// What we say when someone resets a counter after a setback. The whole point is
/// "reset without guilt": every message reframes a relapse as a normal, expected
/// part of recovery — not a failure — and is grounded in a cited source (NIDA,
/// SAMHSA). One is chosen at random on reset.
/// </summary>
public static class RecoveryEncouragement
{
    private const string Nida = "https://nida.nih.gov/publications/drugs-brains-behavior-science-addiction/treatment-recovery";
    private const string SamhsaHelpline = "https://www.samhsa.gov/find-help/national-helpline";

    public static readonly IReadOnlyList<Encouragement> All = new[]
    {
        new Encouragement(
            "A relapse doesn't mean you've failed or that recovery isn't working. For a chronic condition, setbacks are part of the process — a signal to adjust and keep going, not to give up.",
            "NIDA — Treatment and Recovery", Nida),
        new Encouragement(
            "What you built isn't erased. Every day you strung together was real, and you now know from experience that you can do it. Starting again is not starting from zero.",
            "NIDA — Treatment and Recovery", Nida),
        new Encouragement(
            "Be as kind to yourself right now as you would be to a good friend. Asking for support is a strength — SAMHSA's free, confidential helpline is there 24/7, whenever you need it.",
            "SAMHSA — National Helpline", SamhsaHelpline),
    };

    public static Encouragement Random() => All[System.Random.Shared.Next(All.Count)];
}
