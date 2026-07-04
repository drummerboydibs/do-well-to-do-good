namespace DoWellToDoGood.Models;

/// <summary>A belief system a user can opt into for faith-based wellness content.</summary>
public enum FaithTradition
{
    Christianity,
    Islam,
    Hinduism,
    Judaism,
}

/// <summary>
/// A cited passage of scripture or sacred writing. <see cref="Reference"/> is the
/// book/chapter/verse (or tradition-appropriate locator), <see cref="Version"/>
/// the translation, and <see cref="SourceName"/>/<see cref="SourceUrl"/> credit a
/// reputable source. <see cref="Id"/> is stable, for no-repeat tracking.
/// </summary>
public record Passage(
    string Id,
    FaithTradition Tradition,
    string Text,
    string Reference,
    string Version,
    string SourceName,
    string SourceUrl);

/// <summary>
/// Optional, opt-in faith-based content, matched to mood the same way tips are:
/// specific-feeling pool → core-family pool → a general pool. Everything ships in
/// the app (never fetched from a scripture API at runtime — that would leak the
/// user's faith and IP to a third party) and every passage cites its source.
///
/// Content is curated to be wellness-supportive and respectful — comfort, hope,
/// patience, gratitude, compassion — and never polemical toward any group. Only
/// Christianity (KJV, public domain) is stocked so far; more traditions follow.
/// </summary>
public static class FaithLibrary
{
    /// <summary>Traditions that currently have content. The Account selector greys out the rest.</summary>
    public static readonly IReadOnlyList<FaithTradition> Available = new[] { FaithTradition.Christianity };

    public static string DisplayName(FaithTradition t) => t switch
    {
        FaithTradition.Christianity => "Christianity",
        FaithTradition.Islam => "Islam",
        FaithTradition.Hinduism => "Hinduism",
        FaithTradition.Judaism => "Judaism",
        _ => t.ToString(),
    };

    // Deep-links to a reputable, full-text public-domain KJV so each citation is verifiable.
    private static string Bg(string reference) =>
        $"https://www.biblegateway.com/passage/?search={Uri.EscapeDataString(reference)}&version=KJV";

    private static Passage Kjv(string id, string text, string reference) =>
        new(id, FaithTradition.Christianity, text, reference, "KJV", "King James Version · Bible Gateway", Bg(reference));

    // ===================== Christianity (KJV) =====================

    private static readonly Passage[] ChrGeneral =
    {
        Kjv("chr-gen-1", "God is our refuge and strength, a very present help in trouble.", "Psalm 46:1"),
        Kjv("chr-gen-2", "Trust in the LORD with all thine heart; and lean not unto thine own understanding. In all thy ways acknowledge him, and he shall direct thy paths.", "Proverbs 3:5-6"),
        Kjv("chr-gen-3", "But they that wait upon the LORD shall renew their strength; they shall mount up with wings as eagles; they shall run, and not be weary; and they shall walk, and not faint.", "Isaiah 40:31"),
        Kjv("chr-gen-4", "This is the day which the LORD hath made; we will rejoice and be glad in it.", "Psalm 118:24"),
        Kjv("chr-gen-5", "It is of the LORD's mercies that we are not consumed, because his compassions fail not. They are new every morning: great is thy faithfulness.", "Lamentations 3:22-23"),
        Kjv("chr-gen-6", "He hath shewed thee, O man, what is good; and what doth the LORD require of thee, but to do justly, and to love mercy, and to walk humbly with thy God?", "Micah 6:8"),
    };

    private static readonly Passage[] ChrFearful =
    {
        Kjv("chr-fear-1", "Be careful for nothing; but in every thing by prayer and supplication with thanksgiving let your requests be made known unto God. And the peace of God, which passeth all understanding, shall keep your hearts and minds through Christ Jesus.", "Philippians 4:6-7"),
        Kjv("chr-fear-2", "Fear thou not; for I am with thee: be not dismayed; for I am thy God: I will strengthen thee; yea, I will help thee.", "Isaiah 41:10"),
        Kjv("chr-fear-3", "Casting all your care upon him; for he careth for you.", "1 Peter 5:7"),
        Kjv("chr-fear-4", "Take therefore no thought for the morrow: for the morrow shall take thought for the things of itself. Sufficient unto the day is the evil thereof.", "Matthew 6:34"),
    };

    private static readonly Passage[] ChrSad =
    {
        Kjv("chr-sad-1", "The LORD is nigh unto them that are of a broken heart; and saveth such as be of a contrite spirit.", "Psalm 34:18"),
        Kjv("chr-sad-2", "Blessed are they that mourn: for they shall be comforted.", "Matthew 5:4"),
        Kjv("chr-sad-3", "He healeth the broken in heart, and bindeth up their wounds.", "Psalm 147:3"),
        Kjv("chr-sad-4", "And God shall wipe away all tears from their eyes; and there shall be no more death, neither sorrow, nor crying, neither shall there be any more pain.", "Revelation 21:4"),
    };

    private static readonly Passage[] ChrBad =
    {
        Kjv("chr-bad-1", "Come unto me, all ye that labour and are heavy laden, and I will give you rest.", "Matthew 11:28"),
        Kjv("chr-bad-2", "The LORD is my shepherd; I shall not want. He maketh me to lie down in green pastures: he leadeth me beside the still waters. He restoreth my soul.", "Psalm 23:1-3"),
        Kjv("chr-bad-3", "Cast thy burden upon the LORD, and he shall sustain thee: he shall never suffer the righteous to be moved.", "Psalm 55:22"),
    };

    private static readonly Passage[] ChrAngry =
    {
        Kjv("chr-ang-1", "A soft answer turneth away wrath: but grievous words stir up anger.", "Proverbs 15:1"),
        Kjv("chr-ang-2", "Wherefore, my beloved brethren, let every man be swift to hear, slow to speak, slow to wrath: for the wrath of man worketh not the righteousness of God.", "James 1:19-20"),
        Kjv("chr-ang-3", "Be ye angry, and sin not: let not the sun go down upon your wrath.", "Ephesians 4:26"),
    };

    private static readonly Passage[] ChrHappy =
    {
        Kjv("chr-hap-1", "Rejoice evermore. Pray without ceasing. In every thing give thanks: for this is the will of God in Christ Jesus concerning you.", "1 Thessalonians 5:16-18"),
        Kjv("chr-hap-2", "Enter into his gates with thanksgiving, and into his courts with praise: be thankful unto him, and bless his name.", "Psalm 100:4"),
        Kjv("chr-hap-3", "Every good gift and every perfect gift is from above, and cometh down from the Father of lights.", "James 1:17"),
    };

    // Specific-feeling pools (loneliness, guilt/shame).
    private static readonly Passage[] ChrLonely =
    {
        Kjv("chr-lon-1", "Be strong and of a good courage, fear not, nor be afraid of them: for the LORD thy God, he it is that doth go with thee; he will not fail thee, nor forsake thee.", "Deuteronomy 31:6"),
        Kjv("chr-lon-2", "When my father and my mother forsake me, then the LORD will take me up.", "Psalm 27:10"),
    };

    private static readonly Passage[] ChrGuilt =
    {
        Kjv("chr-guilt-1", "As far as the east is from the west, so far hath he removed our transgressions from us.", "Psalm 103:12"),
        Kjv("chr-guilt-2", "If we confess our sins, he is faithful and just to forgive us our sins, and to cleanse us from all unrighteousness.", "1 John 1:9"),
    };

    private static readonly Dictionary<string, Passage[]> ChrByCore = new()
    {
        ["happy"] = ChrHappy,
        ["sad"] = ChrSad,
        ["fearful"] = ChrFearful,
        ["bad"] = ChrBad,
        ["angry"] = ChrAngry,
    };

    private static readonly Dictionary<string, Passage[]> ChrSpecific = new()
    {
        ["lonely"] = ChrLonely, ["isolated"] = ChrLonely, ["abandoned"] = ChrLonely, ["excluded"] = ChrLonely,
        ["guilty"] = ChrGuilt, ["ashamed"] = ChrGuilt, ["remorseful"] = ChrGuilt,
    };

    private static Passage[] General(FaithTradition t) => t switch
    {
        FaithTradition.Christianity => ChrGeneral,
        _ => Array.Empty<Passage>(),
    };

    // ===================== Selection helpers =====================

    /// <summary>
    /// A passage matched to a feeling for one tradition, cascading specific pool →
    /// core-family pool → general, and preferring ids not in <paramref name="recentIds"/>.
    /// Null if the tradition has no content.
    /// </summary>
    public static Passage? PickForEmotion(FaithTradition tradition, EmotionInfo? emotion, IReadOnlyCollection<string>? recentIds = null)
    {
        var candidates = Candidates(tradition, emotion);
        if (candidates.Count == 0) return null;
        var fresh = recentIds is { Count: > 0 }
            ? candidates.Where(p => !recentIds.Contains(p.Id)).ToList()
            : candidates;
        var pool = fresh.Count > 0 ? fresh : candidates;
        return pool[Random.Shared.Next(pool.Count)];
    }

    private static List<Passage> Candidates(FaithTradition tradition, EmotionInfo? e)
    {
        if (tradition != FaithTradition.Christianity) return new();

        var list = new List<Passage>();
        if (e is not null)
        {
            if (ChrSpecific.TryGetValue(e.Name.ToLowerInvariant(), out var specific))
                list.AddRange(specific);
            if (ChrByCore.TryGetValue(e.CoreKey, out var core))
                list.AddRange(core);
        }
        if (list.Count == 0) list.AddRange(ChrGeneral);
        return list.Distinct().ToList();
    }

    /// <summary>
    /// The passage of the day. With more than one tradition selected it rotates
    /// across them by day, so each is represented over time. Deterministic per
    /// calendar day. Null if nothing is selected or stocked.
    /// </summary>
    public static Passage? Daily(IReadOnlyList<FaithTradition> selected, DateOnly date)
    {
        var stocked = selected.Where(t => General(t).Length > 0).ToList();
        if (stocked.Count == 0) return null;

        var day = date.DayNumber;
        var tradition = stocked[(day % stocked.Count + stocked.Count) % stocked.Count];
        var pool = General(tradition);
        // Advance through the pool on a slower cycle than the tradition rotation.
        var idx = (day / stocked.Count) % pool.Length;
        return pool[idx];
    }
}
