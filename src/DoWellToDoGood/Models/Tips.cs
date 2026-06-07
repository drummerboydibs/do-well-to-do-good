namespace DoWellToDoGood.Models;

/// <summary>
/// A wellness tip. <see cref="Text"/> is written in our own words (paraphrased,
/// never copied) and grounded in the cited source; <see cref="SourceName"/> /
/// <see cref="SourceUrl"/> credit and link to the original so users can read more.
/// </summary>
public record Tip(string Text, string SourceName, string SourceUrl);

/// <summary>
/// Tips categorized by core emotion family, each attributed to a reputable,
/// evidence-based source (NHS, NIMH/NIH, APA, Mayo Clinic, UC Berkeley's Greater
/// Good Science Center). URLs verified June 2026.
/// </summary>
public static class TipLibrary
{
    // Reusable source references (real, verified URLs).
    private const string NhsFiveSteps = "https://www.nhs.uk/mental-health/self-help/guides-tools-and-activities/five-steps-to-mental-wellbeing/";
    private const string NhsStress = "https://www.nhs.uk/mental-health/self-help/guides-tools-and-activities/tips-to-reduce-stress/";
    private const string NimhAnxiety = "https://www.nimh.nih.gov/health/topics/anxiety-disorders";
    private const string NimhStress = "https://www.nimh.nih.gov/health/publications/so-stressed-out-fact-sheet";
    private const string NimhCaring = "https://www.nimh.nih.gov/health/topics/caring-for-your-mental-health";
    private const string NimhDepression = "https://www.nimh.nih.gov/health/publications/depression";
    private const string ApaAngerControl = "https://www.apa.org/topics/anger/control";
    private const string ApaAngerStrategies = "https://www.apa.org/topics/anger/strategies-controlling";
    private const string MayoAnger = "https://www.mayoclinic.org/healthy-lifestyle/adult-health/in-depth/anger-management/art-20045434";
    private const string GgSavoring = "https://greatergood.berkeley.edu/article/item/10_steps_to_savoring_the_good_things_in_life";
    private const string GgGratitudeJournal = "https://ggia.berkeley.edu/practice/gratitude_journal";

    private static Tip T(string text, string source, string url) => new(text, source, url);

    private static readonly Dictionary<string, Tip[]> ByCore = new()
    {
        ["happy"] = new[]
        {
            T("Pause and really take this in. Researchers call it “savoring” — lingering on a good moment helps its benefits last longer.", "Greater Good Science Center, UC Berkeley", GgSavoring),
            T("Try jotting down a few things you’re grateful for. A regular gratitude habit is linked to more positive emotion and stronger relationships.", "Greater Good in Action, UC Berkeley", GgGratitudeJournal),
            T("Good feelings often grow when shared. Acts of kindness and giving to others can deepen your own sense of wellbeing.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        },
        ["surprised"] = new[]
        {
            T("The unexpected can take a moment to absorb. A few slow breaths can help you respond thoughtfully rather than react.", "NIMH — Caring for Your Mental Health", NimhCaring),
            T("When things feel up in the air, gently bringing your attention to the present moment can help steady you.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        },
        ["bad"] = new[]
        {
            T("When everything feels like too much, try setting one thing down. Taking even a little control over a problem can ease the pressure.", "NHS — 10 stress busters", NhsStress),
            T("Protecting your sleep and eating regular meals gives you more to cope with. Rest is part of the work, not a reward for finishing it.", "NIMH — I’m So Stressed Out", NimhStress),
            T("A short walk or any movement can lower the intensity of stress and help clear your head.", "NHS — 10 stress busters", NhsStress),
        },
        ["fearful"] = new[]
        {
            T("When worry spikes, slow breathing or a grounding exercise can calm your body’s alarm response.", "NIMH — Anxiety Disorders", NimhAnxiety),
            T("Notice an anxious thought, then gently question it: is it a fact, or a fear? Naming it can cut it down to size.", "NIMH — I’m So Stressed Out", NimhStress),
            T("Reaching out to someone you trust can help carry what feels heavy right now.", "NIMH — Caring for Your Mental Health", NimhCaring),
        },
        ["angry"] = new[]
        {
            T("Before you respond, give yourself a beat. Slowing down and breathing can lower the heat so you can think clearly.", "American Psychological Association", ApaAngerControl),
            T("Anger often rides on something underneath. Naming the need beneath it can help you act on what really matters.", "American Psychological Association", ApaAngerStrategies),
            T("Stepping away or moving your body for a few minutes can take the edge off before you say something you’d take back.", "Mayo Clinic — Anger management", MayoAnger),
        },
        ["disgusted"] = new[]
        {
            T("Strong aversion can be a boundary speaking. Noticing what you want to move away from can clarify what matters to you.", "NIMH — Caring for Your Mental Health", NimhCaring),
            T("Putting a feeling into words — even an uncomfortable one — can make it easier to sit with.", "NIMH — Caring for Your Mental Health", NimhCaring),
        },
        ["sad"] = new[]
        {
            T("Be as kind to yourself as you would be to a friend feeling this way. Low mood is something to meet gently, not a failing.", "NIMH — Depression", NimhDepression),
            T("Even a small dose of movement — like a 30-minute walk — can lift your mood. Doing one thing you used to enjoy helps too, even if you don’t feel like it.", "NIMH — Depression", NimhDepression),
            T("Connection matters most when you feel low. Reaching out to someone you trust, even briefly, can help.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        },
    };

    private static readonly Tip[] Fallback =
    {
        T("Five simple habits support wellbeing: connect with others, stay active, learn something, give to others, and notice the present moment.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("Acts of kindness and giving to others can boost your own sense of purpose — doing good really can help you do well.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("Caring for your mental health is an everyday practice. Small, steady steps add up.", "NIMH — Caring for Your Mental Health", NimhCaring),
    };

    public static Tip Pick(string? coreKey)
    {
        var pool = (coreKey is not null && ByCore.TryGetValue(coreKey, out var tips)) ? tips : Fallback;
        return pool[Random.Shared.Next(pool.Length)];
    }
}
