namespace DoWellToDoGood.Models;

/// <summary>
/// Supplementary mental-wellbeing vocabulary for the "word of the day", beyond
/// the feelings already on the emotion wheel. Plain-language definitions written
/// to be gentle and useful, not clinical.
/// </summary>
public static class WellnessTerms
{
    public static readonly IReadOnlyList<(string Name, string Definition)> All = new (string Name, string Definition)[]
    {
        ("Resilience", "The ability to recover and adapt after hardship or stress — a skill you can build, not a trait you either have or don't."),
        ("Self-compassion", "Treating yourself with the same kindness and patience you'd offer a good friend."),
        ("Mindfulness", "Paying attention to the present moment on purpose, without judging it."),
        ("Gratitude", "Noticing and appreciating the good that's already in your life."),
        ("Savoring", "Deliberately lingering on a positive experience to make its benefits last longer."),
        ("Rumination", "Getting stuck replaying the same distressing thoughts on a loop, without moving toward a solution."),
        ("Burnout", "Emotional and physical exhaustion from prolonged, unrelieved stress."),
        ("Languishing", "A flat, stagnant sense of just getting by — not depressed, but not thriving either."),
        ("Flow", "Being so absorbed and energized by an activity that you lose track of time."),
        ("Equanimity", "A steady, balanced mind that stays grounded through both ups and downs."),
        ("Empathy", "Sensing and sharing in what another person is feeling."),
        ("Compassion", "Caring about someone's suffering and wanting to help ease it."),
        ("Catharsis", "The sense of relief that comes from releasing strong or pent-up emotions."),
        ("Ambivalence", "Holding two opposite feelings about the same thing at once — and that's normal."),
        ("Nostalgia", "A bittersweet, tender fondness for the past."),
        ("Serenity", "A calm, untroubled, peaceful state of mind."),
        ("Grounding", "Using your senses to anchor yourself in the present when emotions surge."),
        ("Boundaries", "The limits you set to protect your time, energy, and wellbeing — a form of self-respect, not selfishness."),
        ("Self-care", "The everyday actions that protect and restore your mental and physical health."),
        ("Perfectionism", "Holding yourself to impossibly high standards and fearing any misstep."),
        ("Procrastination", "Putting off what matters, often to avoid discomfort in the moment."),
        ("Self-efficacy", "Your belief in your own ability to handle what comes your way."),
        ("Presence", "Being fully here with what's happening, rather than lost in the past or future."),
        ("Belonging", "The sense of being accepted and part of something larger than yourself."),
        ("Kindness", "Warmth and generosity toward others — and toward yourself."),
        ("Introspection", "Looking inward to understand your own thoughts, feelings, and motives."),
        ("Contentment", "A quiet, unhurried satisfaction with things as they are right now."),
        ("Reframing", "Deliberately looking at a situation from a new angle to change how it affects you."),
        ("Decompression", "Giving yourself deliberate downtime to unwind after strain."),
        ("Wonder", "Openness to being surprised and moved by the world around you."),
    };
}
