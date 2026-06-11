namespace DoWellToDoGood.Models;

/// <summary>
/// A wellness tip. <see cref="Text"/> is written in our own words (paraphrased,
/// never copied) and grounded in the cited source; <see cref="SourceName"/> /
/// <see cref="SourceUrl"/> credit and link the original so users can read more.
/// <see cref="Id"/> is stable and used for "don't repeat recently" tracking.
/// </summary>
public record Tip(string Id, string Text, string SourceName, string SourceUrl);

/// <summary>
/// Tips matched to the feelings wheel at the most specific level available:
/// exact feeling → parent feeling → core family → general fallback. Every tip
/// is attributed to a reputable, evidence-based source (NHS, NIH/NIMH/NIA, APA,
/// Mayo Clinic, UC Berkeley's Greater Good). URLs verified June 2026.
/// Tips live in the app (not the database) on purpose: querying tips by emotion
/// would tell the server what a user is feeling, which this app must never know.
/// </summary>
public static class TipLibrary
{
    // ---- Verified sources ----
    private const string NhsFiveSteps = "https://www.nhs.uk/mental-health/self-help/guides-tools-and-activities/five-steps-to-mental-wellbeing/";
    private const string NhsStress = "https://www.nhs.uk/mental-health/self-help/guides-tools-and-activities/tips-to-reduce-stress/";
    private const string NhsSleep = "https://www.nhs.uk/every-mind-matters/mental-wellbeing-tips/how-to-fall-asleep-faster-and-sleep-better/";
    private const string NimhAnxiety = "https://www.nimh.nih.gov/health/topics/anxiety-disorders";
    private const string NimhStress = "https://www.nimh.nih.gov/health/publications/so-stressed-out-fact-sheet";
    private const string NimhCaring = "https://www.nimh.nih.gov/health/topics/caring-for-your-mental-health";
    private const string NimhDepression = "https://www.nimh.nih.gov/health/publications/depression";
    private const string NiaLoneliness = "https://www.nia.nih.gov/health/loneliness-and-social-isolation/loneliness-and-social-isolation-tips-staying-connected";
    private const string ApaAngerControl = "https://www.apa.org/topics/anger/control";
    private const string ApaAngerStrategies = "https://www.apa.org/topics/anger/strategies-controlling";
    private const string ApaGrief = "https://www.apa.org/topics/families/grief";
    private const string MayoAnger = "https://www.mayoclinic.org/healthy-lifestyle/adult-health/in-depth/anger-management/art-20045434";
    private const string GgSavoring = "https://greatergood.berkeley.edu/article/item/10_steps_to_savoring_the_good_things_in_life";
    private const string GgGratitudeJournal = "https://ggia.berkeley.edu/practice/gratitude_journal";
    private const string GgSelfCompassion = "https://ggia.berkeley.edu/practice/self_compassion_break";

    private static Tip T(string id, string text, string source, string url) => new(id, text, source, url);

    // ===================== Core family pools =====================

    private static readonly Tip[] Happy =
    {
        T("hap-1", "Pause and really take this in. Researchers call it “savoring” — lingering on a good moment helps its benefits last longer.", "Greater Good Science Center, UC Berkeley", GgSavoring),
        T("hap-2", "Try jotting down a few things you’re grateful for. A regular gratitude habit is linked to more positive emotion and stronger relationships.", "Greater Good in Action, UC Berkeley", GgGratitudeJournal),
        T("hap-3", "Good feelings often grow when shared. Acts of kindness and giving to others can deepen your own sense of wellbeing.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("hap-4", "Take a moment to notice the present — the sights, sounds, and feelings around you. Paying attention on purpose strengthens wellbeing.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
    };

    private static readonly Tip[] Surprised =
    {
        T("sur-1", "The unexpected can take a moment to absorb. A few slow breaths can help you respond thoughtfully rather than react.", "NIMH — Caring for Your Mental Health", NimhCaring),
        T("sur-2", "When things feel up in the air, gently bringing your attention to the present moment can help steady you.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("sur-3", "Big news lands easier with company. Talking it through with someone you trust can help you sort out what it means.", "NIMH — Caring for Your Mental Health", NimhCaring),
    };

    private static readonly Tip[] Bad =
    {
        T("bad-1", "When everything feels like too much, try setting one thing down. Taking even a little control over a problem can ease the pressure.", "NHS — 10 stress busters", NhsStress),
        T("bad-2", "Protecting your sleep and eating regular meals gives you more to cope with. Rest is part of the work, not a reward for finishing it.", "NIMH — I’m So Stressed Out", NimhStress),
        T("bad-3", "A short walk or any movement can lower the intensity of stress and help clear your head.", "NHS — 10 stress busters", NhsStress),
        T("bad-4", "It’s okay to say no. Deciding what must get done and what can wait protects your energy for what matters.", "NIMH — I’m So Stressed Out", NimhStress),
    };

    private static readonly Tip[] Fearful =
    {
        T("fea-1", "When worry spikes, slow breathing or a grounding exercise can calm your body’s alarm response.", "NIMH — Anxiety Disorders", NimhAnxiety),
        T("fea-2", "Notice an anxious thought, then gently question it: is it a fact, or a fear? Naming it can cut it down to size.", "NIMH — I’m So Stressed Out", NimhStress),
        T("fea-3", "Reaching out to someone you trust can help carry what feels heavy right now.", "NIMH — Caring for Your Mental Health", NimhCaring),
        T("fea-4", "Regular movement — even a little — is one of the most reliable ways to take the edge off anxious energy.", "NIMH — Anxiety Disorders", NimhAnxiety),
    };

    private static readonly Tip[] Angry =
    {
        T("ang-1", "Before you respond, give yourself a beat. Slowing down and breathing can lower the heat so you can think clearly.", "American Psychological Association", ApaAngerControl),
        T("ang-2", "Anger often rides on something underneath. Naming the need beneath it can help you act on what really matters.", "American Psychological Association", ApaAngerStrategies),
        T("ang-3", "Stepping away or moving your body for a few minutes can take the edge off before you say something you’d take back.", "Mayo Clinic — Anger management", MayoAnger),
        T("ang-4", "Once you’re calmer, look for one piece of the problem you can actually fix — working the solvable part beats stewing on the whole.", "Mayo Clinic — Anger management", MayoAnger),
    };

    private static readonly Tip[] Disgusted =
    {
        T("dis-1", "Strong aversion can be a boundary speaking. Noticing what you want to move away from can clarify what matters to you.", "NIMH — Caring for Your Mental Health", NimhCaring),
        T("dis-2", "Putting a feeling into words — even an uncomfortable one — can make it easier to sit with.", "NIMH — Caring for Your Mental Health", NimhCaring),
        T("dis-3", "If the judgment is aimed at yourself, try answering it with the kindness you’d offer a friend. Self-compassion beats self-criticism for actually getting through hard moments.", "Greater Good in Action, UC Berkeley", GgSelfCompassion),
    };

    private static readonly Tip[] Sad =
    {
        T("sad-1", "Be as kind to yourself as you would be to a friend feeling this way. Low mood is something to meet gently, not a failing.", "NIMH — Depression", NimhDepression),
        T("sad-2", "Even a small dose of movement — like a 30-minute walk — can lift your mood. Doing one thing you used to enjoy helps too, even if you don’t feel like it.", "NIMH — Depression", NimhDepression),
        T("sad-3", "Connection matters most when you feel low. Reaching out to someone you trust, even briefly, can help.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("sad-4", "Set one small, realistic goal for today — something you can actually finish. Small wins are real wins when you’re low.", "NIMH — Depression", NimhDepression),
    };

    // ===================== Specific-feeling pools =====================

    private static readonly Tip[] Anxiety =
    {
        T("anx-1", "Try a slow count: breathe in for four, out for six, a few times over. Longer exhales signal your body that it’s safe to settle.", "NIMH — Anxiety Disorders", NimhAnxiety),
        T("anx-2", "Worry shrinks when it’s scheduled. Write the worry down now and pick a time to deal with it — your mind can stop holding it.", "NHS — Fall asleep faster and sleep better", NhsSleep),
        T("anx-3", "Anxious energy wants somewhere to go. A brisk walk, stretching, or any movement gives it an exit.", "NIMH — I’m So Stressed Out", NimhStress),
    };

    private static readonly Tip[] Lonely =
    {
        T("lon-1", "Loneliness eases with small, regular contact. One short call or video chat with someone familiar counts for more than it feels like it should.", "National Institute on Aging — Staying Connected", NiaLoneliness),
        T("lon-2", "Joining in around something you already enjoy — a class, a club, a walk group — is one of the most reliable paths back to feeling connected.", "National Institute on Aging — Staying Connected", NiaLoneliness),
        T("lon-3", "Helping someone else is a powerful antidote to isolation — volunteering and small favors build connection in both directions.", "National Institute on Aging — Staying Connected", NiaLoneliness),
    };

    private static readonly Tip[] Grief =
    {
        T("gri-1", "Grief has no schedule. Letting it move at its own pace — without judging yourself for how long it takes — is part of healing.", "American Psychological Association — Grief", ApaGrief),
        T("gri-2", "Sharing memories with others who loved them — stories, photos, their favorite music — helps everyone carry the loss together.", "American Psychological Association — Grief", ApaGrief),
        T("gri-3", "Honoring who you lost in a small, concrete way — a ritual, a donation, something planted — can give love somewhere to go.", "American Psychological Association — Grief", ApaGrief),
    };

    private static readonly Tip[] Sleep =
    {
        T("slp-1", "A wind-down routine — same time, gentle activity, screens away — teaches your body when to power down.", "NHS — Fall asleep faster and sleep better", NhsSleep),
        T("slp-2", "If your mind races at night, park it on paper: jot tomorrow’s to-dos before bed so your brain can clock out.", "NHS — Fall asleep faster and sleep better", NhsSleep),
        T("slp-3", "Quiet, dark, and cool is the recipe for restful sleep — and something calm like reading or soft music beats scrolling.", "NHS — Fall asleep faster and sleep better", NhsSleep),
    };

    private static readonly Tip[] SelfCompassion =
    {
        T("cmp-1", "Talk to yourself the way you’d talk to a friend in this exact spot. People who meet their own struggles with kindness recover better than those who self-criticize.", "Greater Good in Action, UC Berkeley", GgSelfCompassion),
        T("cmp-2", "Try naming it plainly: “This is a moment of struggle — and struggling is part of being human.” You’re not the only one who’s been here.", "Greater Good in Action, UC Berkeley", GgSelfCompassion),
        T("cmp-3", "Guilt is information, not a verdict. Take what it’s teaching you, make what repair you can, and let the rest go.", "NIMH — Caring for Your Mental Health", NimhCaring),
    };

    private static readonly Tip[] Stress =
    {
        T("str-1", "Break the mountain into pebbles: list what’s on you, pick the single next step, and do only that.", "NHS — 10 stress busters", NhsStress),
        T("str-2", "Decide what actually must happen today — and give yourself real permission to let the rest wait.", "NIMH — I’m So Stressed Out", NimhStress),
        T("str-3", "Build in something that isn’t work: a few minutes of something you enjoy is maintenance, not indulgence.", "NIMH — I’m So Stressed Out", NimhStress),
    };

    private static readonly Tip[] Bored =
    {
        T("bor-1", "Boredom is often hunger for something new. Learning anything — a recipe, a skill, a few words of a language — feeds it and builds confidence.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("bor-2", "Set yourself a small challenge with a finish line. Goals you can actually reach are fuel for feeling engaged again.", "NHS — 10 stress busters", NhsStress),
    };

    private static readonly Tip[] Uplift =
    {
        T("upl-1", "Tonight, write down three things that went well today and why. It’s a tiny habit with an outsized effect on wellbeing.", "Greater Good in Action, UC Berkeley", GgGratitudeJournal),
        T("upl-2", "Stretch this feeling: tell someone about it, replay it, or note what made it happen. Shared and savored joys last longer.", "Greater Good Science Center, UC Berkeley", GgSavoring),
        T("upl-3", "Riding a good wave is the perfect time to do good — kindness given when you’re up lifts someone else and keeps your own momentum going.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
    };

    private static readonly Tip[] Fallback =
    {
        T("gen-1", "Five simple habits support wellbeing: connect with others, stay active, learn something, give to others, and notice the present moment.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("gen-2", "Acts of kindness and giving to others can boost your own sense of purpose — doing good really can help you do well.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
        T("gen-3", "Caring for your mental health is an everyday practice. Small, steady steps add up.", "NIMH — Caring for Your Mental Health", NimhCaring),
        T("gen-4", "However today went, getting your thoughts out — like you just did — is a real act of self-care.", "NIMH — Caring for Your Mental Health", NimhCaring),
        T("gen-5", "A few quiet minutes of noticing your breath or surroundings counts. Mindful attention, even briefly, supports wellbeing.", "NHS — 5 steps to mental wellbeing", NhsFiveSteps),
    };

    // ===================== Lookup =====================

    private static readonly Dictionary<string, Tip[]> ByCore = new()
    {
        ["happy"] = Happy,
        ["surprised"] = Surprised,
        ["bad"] = Bad,
        ["fearful"] = Fearful,
        ["angry"] = Angry,
        ["disgusted"] = Disgusted,
        ["sad"] = Sad,
    };

    /// <summary>Specific feelings (secondary/tertiary names, lowercased) → focused pools.</summary>
    private static readonly Dictionary<string, Tip[]> Specific = new()
    {
        ["anxious"] = Anxiety, ["worried"] = Anxiety, ["nervous"] = Anxiety,
        ["scared"] = Anxiety, ["frightened"] = Anxiety, ["overwhelmed"] = Stress,
        ["insecure"] = SelfCompassion, ["inadequate"] = SelfCompassion, ["inferior"] = SelfCompassion,
        ["worthless"] = SelfCompassion, ["guilty"] = SelfCompassion, ["ashamed"] = SelfCompassion,
        ["remorseful"] = SelfCompassion, ["embarrassed"] = SelfCompassion, ["judgmental"] = SelfCompassion,
        ["lonely"] = Lonely, ["isolated"] = Lonely, ["abandoned"] = Lonely, ["excluded"] = Lonely,
        ["withdrawn"] = Lonely, ["distant"] = Lonely,
        ["grief"] = Grief, ["despair"] = Grief, ["hurt"] = Grief,
        ["tired"] = Sleep, ["sleepy"] = Sleep, ["unfocused"] = Sleep,
        ["stressed"] = Stress, ["pressured"] = Stress, ["rushed"] = Stress,
        ["busy"] = Stress, ["out of control"] = Stress,
        ["bored"] = Bored, ["indifferent"] = Bored, ["apathetic"] = Bored,
        ["hopeful"] = Uplift, ["optimistic"] = Uplift, ["inspired"] = Uplift,
        ["excited"] = Uplift, ["content"] = Uplift, ["peaceful"] = Uplift,
        ["thankful"] = Uplift, ["loving"] = Uplift, ["proud"] = Uplift,
        ["playful"] = Uplift, ["joyful"] = Uplift, ["curious"] = Uplift,
        ["interested"] = Uplift, ["trusting"] = Uplift, ["accepted"] = Uplift,
    };

    /// <summary>
    /// Pick a tip for a feeling, preferring ones not in <paramref name="recentIds"/>.
    /// Candidates cascade: exact feeling pool → parent feeling pool → core family
    /// pool; general fallback when nothing matches (or no feeling was named).
    /// </summary>
    public static Tip Pick(EmotionInfo? emotion, IReadOnlyCollection<string>? recentIds = null)
    {
        var candidates = Candidates(emotion);
        var fresh = recentIds is { Count: > 0 }
            ? candidates.Where(t => !recentIds.Contains(t.Id)).ToList()
            : candidates;
        var pool = fresh.Count > 0 ? fresh : candidates;
        return pool[Random.Shared.Next(pool.Count)];
    }

    private static List<Tip> Candidates(EmotionInfo? e)
    {
        if (e is null) return Fallback.ToList();

        var list = new List<Tip>();
        if (Specific.TryGetValue(e.Name.ToLowerInvariant(), out var exact))
            list.AddRange(exact);

        // Tertiary feelings also draw on their parent's pool (Trail = "Core › Parent").
        if (e.Tier == 3 && e.Trail.Contains('›'))
        {
            var parent = e.Trail.Split('›')[^1].Trim().ToLowerInvariant();
            if (Specific.TryGetValue(parent, out var parentPool))
                list.AddRange(parentPool);
        }

        if (ByCore.TryGetValue(e.CoreKey, out var core))
            list.AddRange(core);

        return list.Count > 0 ? list.Distinct().ToList() : Fallback.ToList();
    }
}
