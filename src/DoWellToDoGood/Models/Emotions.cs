namespace DoWellToDoGood.Models;

/// <summary>
/// A flattened, selectable feeling. Valence runs -1 (very negative) to +1
/// (very positive) and drives the blue↔red entry tint. ColorKey maps to a CSS
/// custom property (--color-emo-{ColorKey}). Tier: 1 core, 2 secondary, 3 tertiary.
/// Trail is the breadcrumb of ancestors (e.g. "Happy › Content").
/// </summary>
public record EmotionInfo(
    string Name,
    string Definition,
    double Valence,
    string ColorKey,
    string CoreName,
    string CoreKey,
    int Tier,
    string Trail);

public record TertiaryEmotion(string Name, string Definition);

public record SecondaryEmotion(string Name, string Definition, double Valence, IReadOnlyList<TertiaryEmotion> Tertiaries);

public record CoreEmotion(string Key, string Name, string Definition, double Valence, string ColorKey, IReadOnlyList<SecondaryEmotion> Secondaries);

/// <summary>
/// The full three-ring feelings-wheel taxonomy (Geoffrey Roberts / Gloria Willcox
/// lineage). Family colors follow the app's Inside-Out palette. Definitions are
/// written in plain language to help people name what they feel.
/// </summary>
public static class EmotionCatalog
{
    private static TertiaryEmotion T(string name, string def) => new(name, def);
    private static SecondaryEmotion Sec(string name, string def, double valence, params TertiaryEmotion[] tertiaries)
        => new(name, def, valence, tertiaries);

    public static readonly IReadOnlyList<CoreEmotion> Cores = new[]
    {
        new CoreEmotion("happy", "Happy", "A general sense of pleasure, contentment, or wellbeing.", 0.85, "happy", new[]
        {
            Sec("Playful", "Lighthearted and up for fun.", 0.70, T("Aroused", "Lively and stimulated."), T("Cheeky", "Playfully bold or mischievous.")),
            Sec("Content", "At ease and satisfied with how things are.", 0.80, T("Free", "Unburdened and unconstrained."), T("Joyful", "Bright, lively happiness.")),
            Sec("Interested", "Curious and engaged.", 0.60, T("Curious", "Eager to learn or explore."), T("Inquisitive", "Keen to ask and find out.")),
            Sec("Proud", "Pleased with something achieved.", 0.75, T("Successful", "Having accomplished a goal."), T("Confident", "Sure of yourself.")),
            Sec("Accepted", "Valued and included.", 0.70, T("Respected", "Held in regard by others."), T("Valued", "Treated as important.")),
            Sec("Powerful", "Capable and effective.", 0.70, T("Courageous", "Willing to face difficulty."), T("Creative", "Full of new ideas.")),
            Sec("Peaceful", "Calm and untroubled.", 0.85, T("Loving", "Warm and affectionate."), T("Thankful", "Grateful for what you have.")),
            Sec("Trusting", "Open and able to rely on others.", 0.70, T("Sensitive", "Attuned and easily moved."), T("Intimate", "Emotionally close.")),
            Sec("Optimistic", "Expecting good things.", 0.80, T("Hopeful", "Trusting that things can improve."), T("Inspired", "Moved to create or act.")),
        }),

        new CoreEmotion("surprised", "Surprised", "A reaction to something sudden or unexpected.", 0.20, "surprise", new[]
        {
            Sec("Excited", "Eager and enthusiastic.", 0.65, T("Eager", "Keen and impatient to act."), T("Energetic", "Full of energy.")),
            Sec("Amazed", "Filled with wonder.", 0.50, T("Astonished", "Greatly surprised."), T("Awe", "Overwhelmed by something vast or grand.")),
            Sec("Confused", "Unable to make sense of things.", -0.30, T("Disillusioned", "Let down after losing a belief."), T("Perplexed", "Puzzled and unsure.")),
            Sec("Startled", "Briefly shocked by something sudden.", -0.10, T("Shocked", "Jolted by something unexpected."), T("Dismayed", "Unsettled and discouraged.")),
        }),

        new CoreEmotion("bad", "Bad", "A depleted, low-energy unease — stretched thin.", -0.45, "bad", new[]
        {
            Sec("Bored", "Restless and unstimulated.", -0.30, T("Indifferent", "Lacking interest or concern."), T("Apathetic", "Without motivation or feeling.")),
            Sec("Busy", "Overloaded with too much to do.", -0.30, T("Pressured", "Pushed by demands."), T("Rushed", "Hurried with too little time.")),
            Sec("Stressed", "Under strain and pressure.", -0.60, T("Overwhelmed", "Unable to cope with it all."), T("Out of control", "Unable to steer what's happening.")),
            Sec("Tired", "Low on energy; needing rest.", -0.40, T("Sleepy", "Drowsy and heavy."), T("Unfocused", "Unable to concentrate.")),
        }),

        new CoreEmotion("fearful", "Fearful", "Unease or alarm at a real or imagined threat.", -0.70, "fear", new[]
        {
            Sec("Scared", "Frightened by a threat.", -0.70, T("Helpless", "Unable to protect yourself or act."), T("Frightened", "Sharply afraid.")),
            Sec("Anxious", "Uneasy worry about what may come.", -0.60, T("Overwhelmed", "Swamped by it all."), T("Worried", "Troubled by anxious thoughts.")),
            Sec("Insecure", "Unsure of your worth or safety.", -0.60, T("Inadequate", "Feeling not enough."), T("Inferior", "Feeling lesser than others.")),
            Sec("Weak", "Lacking strength or power.", -0.60, T("Worthless", "Feeling of no value."), T("Insignificant", "Feeling unimportant.")),
            Sec("Rejected", "Pushed away or unwanted.", -0.70, T("Excluded", "Left out."), T("Persecuted", "Unfairly targeted.")),
            Sec("Threatened", "Sensing danger or risk.", -0.65, T("Nervous", "Tense and apprehensive."), T("Exposed", "Unprotected and vulnerable.")),
        }),

        new CoreEmotion("angry", "Angry", "Strong displeasure, often from feeling wronged.", -0.65, "angry", new[]
        {
            Sec("Let down", "Disappointed by someone you relied on.", -0.60, T("Betrayed", "Hurt by broken trust."), T("Resentful", "Bitter over unfair treatment.")),
            Sec("Humiliated", "Shamed in front of others.", -0.70, T("Disrespected", "Treated as if you don't matter."), T("Ridiculed", "Mocked or made fun of.")),
            Sec("Bitter", "Resentful and hardened.", -0.60, T("Indignant", "Angered by unfairness."), T("Violated", "Boundaries crossed.")),
            Sec("Mad", "Strongly angry.", -0.70, T("Furious", "Intensely angry."), T("Jealous", "Threatened over something you value.")),
            Sec("Aggressive", "Forceful and confrontational.", -0.70, T("Provoked", "Pushed to react."), T("Hostile", "Openly antagonistic.")),
            Sec("Frustrated", "Blocked from making progress.", -0.50, T("Infuriated", "Made very angry."), T("Annoyed", "Mildly bothered.")),
            Sec("Distant", "Pulled back and detached.", -0.50, T("Withdrawn", "Retreated from others."), T("Numb", "Feeling little or nothing.")),
            Sec("Critical", "Finding fault.", -0.50, T("Sceptical", "Doubtful and questioning."), T("Dismissive", "Treating as unworthy of attention.")),
        }),

        new CoreEmotion("disgusted", "Disgusted", "Strong aversion or distaste.", -0.60, "disgust", new[]
        {
            Sec("Disapproving", "Judging something as wrong.", -0.50, T("Judgmental", "Harshly evaluating."), T("Embarrassed", "Self-conscious and awkward.")),
            Sec("Disappointed", "Let down by an outcome.", -0.60, T("Appalled", "Deeply shocked."), T("Revolted", "Sickened and repelled.")),
            Sec("Awful", "Feeling sickened or terrible.", -0.70, T("Nauseated", "Queasy with disgust."), T("Detestable", "Finding something hateful.")),
            Sec("Repelled", "Wanting to pull away.", -0.60, T("Horrified", "Filled with horror."), T("Hesitant", "Holding back, uneasy.")),
        }),

        new CoreEmotion("sad", "Sad", "Low spirits from loss or unmet needs.", -0.80, "sad", new[]
        {
            Sec("Lonely", "Isolated or disconnected from others.", -0.70, T("Isolated", "Cut off from others."), T("Abandoned", "Left alone or deserted.")),
            Sec("Vulnerable", "Exposed and easily hurt.", -0.55, T("Victimised", "Wronged or taken advantage of."), T("Fragile", "Delicate and easily broken.")),
            Sec("Despair", "A loss of hope.", -0.85, T("Grief", "Deep sorrow after a loss."), T("Powerless", "Unable to change things.")),
            Sec("Guilty", "Distress over a wrong you did.", -0.60, T("Ashamed", "Painful sense of having failed."), T("Remorseful", "Regretful for what you did.")),
            Sec("Depressed", "Persistent, heavy low mood.", -0.80, T("Empty", "Hollow and without feeling."), T("Inferior", "Feeling lesser than others.")),
            Sec("Hurt", "Emotional pain.", -0.70, T("Embarrassed", "Self-conscious and exposed."), T("Disappointed", "Let down.")),
        }),
    };

    /// <summary>All feeling names across every ring, for the autocomplete datalist.</summary>
    public static IEnumerable<string> AllNames()
        => Cores.SelectMany(c => new[] { c.Name }
                .Concat(c.Secondaries.SelectMany(s => new[] { s.Name }.Concat(s.Tertiaries.Select(t => t.Name)))))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase);

    /// <summary>Look up any feeling by name (case-insensitive). Returns null for free text.</summary>
    public static EmotionInfo? Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        foreach (var c in Cores)
        {
            foreach (var s in c.Secondaries)
            {
                foreach (var t in s.Tertiaries)
                    if (Eq(t.Name, name))
                        return new EmotionInfo(t.Name, t.Definition, s.Valence, c.ColorKey, c.Name, c.Key, 3, $"{c.Name} › {s.Name}");
                if (Eq(s.Name, name))
                    return new EmotionInfo(s.Name, s.Definition, s.Valence, c.ColorKey, c.Name, c.Key, 2, c.Name);
            }
            if (Eq(c.Name, name))
                return new EmotionInfo(c.Name, c.Definition, c.Valence, c.ColorKey, c.Name, c.Key, 1, "");
        }
        return null;
    }

    private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
