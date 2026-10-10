using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.LobbyServer.Event;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.Utils;

public class GachaUtils
{
    private static readonly List<int> sickPullsExclusionList = [2500601]; // Add more IDs as needed
    private static readonly Random random = new();


    /// <summary>
    /// Performs gacha pulls based on the GachaTypeRecord. Handles rates for groups (R, SR, SSR, etc) and attached character lists.
    /// </summary>
    /// <param name="gachaType"></param>
    /// <param name="numberOfPulls"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static List<CharacterRecord> ExecuteGachaPull(GachaTypeRecord gachaType, int numberOfPulls, User user)
    {
        List<CharacterRecord> entireallCharacterData = [.. GameData.Instance.CharacterTable.Values];

        // Fill the wishlist anyway. It won't be used of not compatible with the banner.
        List<CharacterRecord> wishlistCharacters = user.GetWishlistCharacters(gachaType.Id);

        List<CharacterRecord> selectedCharacters = [];

        if (user.sickpulls && gachaType.Type != GachaPremiumType.GachaFriend) // Left this in for backward compatibility with previous code. The friend banner handles sickpulls in SelectRandomCharacter.
        {
            // Remove the .Values part since it's already a list.
            // Group by NameCode to treat same NameCode as one character 
            // Always add characters with GradeCoreId == 1 and 101
            List<CharacterRecord> allCharacterData = [.. entireallCharacterData.GroupBy(c => c.NameCode).SelectMany(g => g.Where(c => c.GradeCoreId == 1 || c.GradeCoreId == 101 || c.GradeCoreId == 201 || c.NameCode == 3999))];

            // Old selection method: Randomly select characters based on req.Count value, excluding characters in the sickPullsExclusionList
            selectedCharacters = [.. allCharacterData.Where(c => !sickPullsExclusionList.Contains(c.Id)).OrderBy(x => random.Next()).Take(numberOfPulls)]; // Exclude characters based on the exclusion list for sick pulls
        }
        else if (gachaType.Type == GachaPremiumType.GachaTutorial)
        {
            if (numberOfPulls != 10)
            {
                Logging.WriteLine("[SelectRandomCharacter] Tutorial Gacha Banners must have 10 pulls", LogType.Error);
                throw new ArgumentException("Tutorial Gacha Banners must have 10 pulls");
            }

            // This is the tutorial banner. It gives only one SSR, no more, no less.
            // To make it interesting for the user, select a random spot for the SSR pull
            // We handle it manually because the GachaProbRecord for SSR has a zero probability. It is most likely used by the game for display rather than any processing.
            // Everything else can be handled by the grade probabilities provided by the game files.
            long ssrSpot = random.NextInt64(0, 10);

            for (int i = 0; i < numberOfPulls; i++)
            {
                if (i == ssrSpot)
                {
                    GachaGradeProbRecord gradeProbs = GameData.Instance.GachaGradeProb.Where(p => p.Value.GroupId == gachaType.GradeProbId)
                                                                                                        .Where(p => p.Value.Rare == OriginalRareType.SSR).Select(p => p.Value).First();
                    CharacterRecord character = SelectRandomCharacterFromProbList(gradeProbs, wishlistCharacters, user);
                    selectedCharacters.Add(character);
                }
                else
                {
                    // The gacha type record has a zero probability for SSR, so SSRs won't 
                    CharacterRecord character = SelectRandomCharacter(gachaType, wishlistCharacters, user);
                    selectedCharacters.Add(character);
                }
            }
        }
        else
        {
            // Now using the game probability tables to do the pulls
            for (int i = 0; i < numberOfPulls; i++)
            {
                // Added bannerID to the SelectRandomCharacter method to allow for specific logic such as wishlist handling
                CharacterRecord character = SelectRandomCharacter(gachaType, wishlistCharacters, user);
                selectedCharacters.Add(character);
            }
        }

        return selectedCharacters;
    }


    /// <summary>
    /// Overall SSR chance on the friend banner (0.13 = 13%). Only raised, never lowered.
    /// </summary>
    private const double LimitedSsrRate = 0.055;

    /// <summary>
    /// Same as LimitedSsrRate but when user.sickpulls is on. Ignores the normal rate (1.0 = every pull is SSR).
    /// </summary>
    private const double LimitedSsrRateSickPulls = 1.0;

    private static readonly Dictionary<OriginalRareType, HashSet<int>> standardPools = [];
    private static readonly Dictionary<int, bool> limitedBannerCache = [];

    /// <summary>
    /// Units the standard banner can give at this rarity (for SSR: the wishlistable units).
    /// </summary>
    private static HashSet<int> GetStandardPool(OriginalRareType rare)
    {
        if (standardPools.TryGetValue(rare, out var cached)) return cached;

        var data = GameData.Instance;
        var standardBanner = data.gachaTypes.Values.FirstOrDefault(g => g.Type == GachaPremiumType.GachaPremium);

        HashSet<int> pool = [];
        if (standardBanner != null)
        {
            pool = data.GachaGradeProb.Values
                .Where(p => p.GroupId == standardBanner.GradeProbId && p.Rare == rare)
                .SelectMany(p => data.GachaListProb.Values.Where(l => l.GroupId == p.GachaListId).Select(l => l.GachaId))
                .ToHashSet();
        }

        standardPools[rare] = pool;
        return pool;
    }

    /// <summary>
    /// Limited unit = not part of the standard pool of its own rarity.
    /// </summary>
    private static bool IsLimitedUnit(int characterId)
    {
        return GameData.Instance.CharacterTable.TryGetValue(characterId, out var ch)
               && !GetStandardPool(ch.OriginalRare).Contains(characterId);
    }

    /// <summary>
    /// A pickup banner is limited when one of its rate-up units (any rarity) is a limited unit.
    /// The banner type alone cannot tell, permanent and limited banners are both pickup.
    /// </summary>
    private static bool IsLimitedBanner(GachaTypeRecord gachaType)
    {
        if (limitedBannerCache.TryGetValue(gachaType.Id, out bool cached)) return cached;

        bool limited = gachaType.Type == GachaPremiumType.GachaPickup
            && gachaType.PickupCharGroupId != 0
            && GameData.Instance.GachaListProb.Values
                .Where(l => l.GroupId == gachaType.PickupCharGroupId
                            && (l.GachaSubType is GachaSubType.PickupCharacter or GachaSubType.LimitedCharacter))
                .Any(l => IsLimitedUnit(l.GachaId));

        limitedBannerCache[gachaType.Id] = limited;
        return limited;
    }
    
    /// <summary>
    /// Rate-up units of the currently active event banners, unordered.
    /// </summary>
    public static HashSet<int> GetActiveRateUpIds(User user) => [.. GetActiveRateUpIdsOrdered(user)];

    /// <summary>
    /// Rate-up units of the currently active event banners, in display order:
    /// newest event first (highest pickup gacha event id), and inside an event pickup units before selectup units.
    /// Chain: banner event -> child events -> shared banner resource table -> pickup gacha event -> GachaType.PickupCharGroupId -> GachaListProb.GroupId
    /// Selectup banners use GachaSelectupListTable (all choices, not the user's pick).
    /// </summary>
    public static List<int> GetActiveRateUpIdsOrdered(User user)
    {
        var data = GameData.Instance;
        var eventManagers = data.eventManagers.Values;
        var perBanner = new List<(int EventKey, List<int> Units)>();

        foreach (var banner in EventHelper.GetLobbyPrivateBannerData(user))
        {
            var resourceTables = eventManagers
                .Where(em => (em.SetField == banner.EventId || em.ParentsEventId == banner.EventId)
                             && em.EventBannerResourceTable != null
                             && em.EventBannerResourceTable.StartsWith("event_"))
                .Select(em => em.EventBannerResourceTable)
                .ToHashSet();

            var gachaEventIds = eventManagers
                .Where(em => em.EventSystemType == EventSystemType.PickupGachaEvent
                             && em.EventBannerResourceTable != null
                             && resourceTables.Contains(em.EventBannerResourceTable))
                .Select(em => em.Id)
                .ToHashSet();

            var units = new List<int>();
            foreach (var gacha in data.gachaTypes.Values
                         .Where(g => gachaEventIds.Contains(g.EventId))
                         .OrderBy(g => g.Type == GachaPremiumType.GachaSelectup ? 1 : 0) // pickup first, then selectup
                         .ThenBy(g => g.Id))
            {
                if (gacha.Type == GachaPremiumType.GachaSelectup)
                {
                    units.AddRange(data.GachaSelectupListTable.Values
                        .Where(s => s.GachaTypeId == gacha.Id)
                        .OrderBy(s => s.Id)
                        .Select(s => s.CharacterId));
                }
                else if (gacha.Type == GachaPremiumType.GachaPickup && gacha.PickupCharGroupId != 0)
                {
                    units.AddRange(data.GachaListProb.Values
                        .Where(l => l.GroupId == gacha.PickupCharGroupId && l.GachaSubType == GachaSubType.PickupCharacter)
                        .OrderBy(l => l.Id)
                        .Select(l => l.GachaId));
                }
            }

            perBanner.Add((gachaEventIds.Count == 0 ? 0 : gachaEventIds.Max(), units));
        }

        var result = new List<int>();
        var seen = new HashSet<int>();
        foreach (var entry in perBanner.OrderByDescending(b => b.EventKey)) // newest event first
            foreach (var id in entry.Units)
                if (seen.Add(id))
                    result.Add(id);
        return result;
    }

    private static Dictionary<int, GachaListProbRecord>? limitedUnitRows;

    /// <summary>
    /// Every unit that appears as rate-up/limited in the gacha lists and is outside the standard pool of its rarity.
    /// One row per unit, keeps the rate-up weight. Computed once.
    /// </summary>
    private static Dictionary<int, GachaListProbRecord> GetAllLimitedUnitRows()
    {
        return limitedUnitRows ??= GameData.Instance.GachaListProb.Values
            .Where(r => r.GachaType == GachaCategory.Character
                        && (r.GachaSubType is GachaSubType.PickupCharacter or GachaSubType.LimitedCharacter)
                        && IsLimitedUnit(r.GachaId))
            .GroupBy(r => r.GachaId)
            .ToDictionary(g => g.Key, g => g.First());
    }

    /// <summary>
    /// Candidates for the limited pool of the given rarity:
    /// all limited units (outside the standard pool) plus the rate-up units of the active event banners (even permanent ones).
    /// Keys and Ids are negative so they can never collide with real GachaListProb record ids.
    /// </summary>
    private static Dictionary<int, GachaListProbRecord> GetLimitedCandidates(User user, OriginalRareType rare, int defaultProb)
    {
        var data = GameData.Instance;
        var result = new Dictionary<int, GachaListProbRecord>();
        var limitedRows = GetAllLimitedUnitRows();

        var ids = new HashSet<int>(limitedRows.Keys);
        ids.UnionWith(GetActiveRateUpIds(user));

        foreach (var id in ids)
        {
            if (!data.CharacterTable.TryGetValue(id, out var ch) || ch.OriginalRare != rare)
                continue;

            limitedRows.TryGetValue(id, out var row);
            row ??= data.GachaListProb.Values.FirstOrDefault(r =>
                r.GachaId == id && r.GachaType == GachaCategory.Character &&
                (r.GachaSubType is GachaSubType.PickupCharacter or GachaSubType.LimitedCharacter));

            result[-id] = new GachaListProbRecord
            {
                Id = -id,
                GroupId = row?.GroupId ?? 0,
                GachaType = GachaCategory.Character,
                GachaSubType = GachaSubType.None,
                GachaId = id,
                Prob = row?.Prob ?? defaultProb,
            };
        }
        return result;
    }

    private static CharacterRecord SelectRandomCharacter(GachaTypeRecord gachaType, List<CharacterRecord> wishlistCharacters, User user)
    {

        // Load the categories and they probabilities
        // This table holds a link to premade lists for each grade and banner. No need to check each character type individually
        Dictionary<int, GachaGradeProbRecord> gradeProbs = GameData.Instance.GachaGradeProb.Where(p => p.Value.GroupId == gachaType.GradeProbId).OrderBy(p => p.Value.Prob).ToDictionary();

        // Limited mode: friend banner only. Every other banner keeps its original behavior.
        bool limitedMode = gachaType.Type == GachaPremiumType.GachaFriend;
        var limitedSsr = limitedMode ? GetLimitedCandidates(user, OriginalRareType.SSR, 100) : [];

        // Build a probability table for the grades
        Dictionary<int, int> gradeWeights = gradeProbs.Values.ToDictionary(g => g.Id, g => g.Prob);
        int baseTotal = gradeWeights.Values.Sum();

        if (limitedSsr.Count > 0)
        {
            // Raise the overall SSR chance to LimitedSsrRate. SSR rows keep their split (rate-up vs rest), R/SR keep theirs.
            int ssrSum = gradeProbs.Values.Where(g => g.Rare == OriginalRareType.SSR).Sum(g => g.Prob);
            int otherSum = baseTotal - ssrSum;
            double ssrRate = user.sickpulls ? LimitedSsrRateSickPulls : LimitedSsrRate;
            double targetSsr = ssrRate * baseTotal;

            if (ssrSum > 0 && otherSum > 0 && targetSsr > ssrSum)
            {
                double ssrFactor = targetSsr / ssrSum;
                double otherFactor = (baseTotal - targetSsr) / otherSum;
                foreach (var g in gradeProbs.Values)
                {
                    int w = (int)Math.Round(g.Prob * (g.Rare == OriginalRareType.SSR ? ssrFactor : otherFactor));
                    // At 100% the non-SSR grades drop to zero, otherwise every grade keeps a minimum weight
                    gradeWeights[g.Id] = ssrRate >= 1.0 ? (g.Rare == OriginalRareType.SSR ? Math.Max(1, w) : 0) : Math.Max(1, w);
                }
            }
        }

        int maxProb = gradeWeights.Values.Sum();

        Dictionary<int, (int minProbInc, int maxProbEx)> gradeProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        int curVal = 0;

        foreach (GachaGradeProbRecord gradeProb in gradeProbs.Values)
        {
            gradeProbsTable.Add(gradeProb.Id, new(curVal, curVal + gradeWeights[gradeProb.Id]));
            curVal += gradeWeights[gradeProb.Id];
        }

        // Now do the roll for the grades
        int gradeRoll = (int)random.NextInt64(maxProb);

        GachaGradeProbRecord selectedGrade = gradeProbs[gradeProbsTable.Where(p => gradeRoll >= p.Value.minProbInc && gradeRoll < p.Value.maxProbEx).Select(p => p.Key).First()];

        // We have the grade, we need to pull the list of characters from GachaListProbTable and create a similar probability table
        // Prefered characters from special banner should be handled automatically by these lists
        // The maximum random value will change depending on how many characters are in the list so we need to keep track of it
        // selectedGrade.GachaListId != selectedGrade.CustomizeListId seem to indicate that wishlisting is supported by the banner/grade. CustomizeListId 19995 and 19996 are most likely SSR and Pilgrims wishlists 

        Dictionary<int, GachaListProbRecord> charProbs = new();

        // Process the wishlist here
        // We filter the character from the category by the ones in the wishlist.
        // CustomizeListId seem to indicate that a wishlist can be specified. It seem to be only different for the two SSR groups in standard banners.
        // Wishlist must not be empty and must have all slots filled in.
        if (wishlistCharacters != null && wishlistCharacters.Count == 20 && selectedGrade.GachaListId != selectedGrade.CustomizeListId && selectedGrade.CustomizeListId != 0)
        {
            Logging.WriteLine($"Using wishlisted characters.");
            int[] ids = wishlistCharacters.Select(c => c.Id).ToArray();
            charProbs = GameData.Instance.GachaListProb.Where(g => g.Value.GroupId == selectedGrade.GachaListId).Where(g => ids.Contains(g.Value.GachaId)).ToDictionary();
        }
        // Otherwise, proceed with regular gacha list
        else
        {
            Logging.WriteLine($"Not using wishlisted characters.{(wishlistCharacters == null || wishlistCharacters.Count != 20 ? " Invalid wishlist." : "")}{(selectedGrade.GachaListId == selectedGrade.CustomizeListId || selectedGrade.CustomizeListId == 0 ? " Invalid Banner or Grade." : "")}");
            charProbs = GameData.Instance.GachaListProb.Where(g => g.Value.GroupId == selectedGrade.GachaListId).ToDictionary();
        }

        // Limited handling: friend banner only.
        // SSR roll only gives limited units (rate-up units of active banners outside the standard pool).
        // SR keeps its normal list and additionally gets limited SR units. R is untouched.
        bool normalList = charProbs.Values.All(r => r.GachaType == GachaCategory.Character && r.GachaSubType == GachaSubType.None);

        if (limitedMode && normalList)
        {
            if (selectedGrade.Rare == OriginalRareType.SSR)
            {
                if (limitedSsr.Count > 0)
                    charProbs = limitedSsr;
                else
                    Logging.WriteLine($"[SelectRandomCharacter] No limited SSR units on active banners for gacha {gachaType.Id}, using the normal list.");
            }
            else if (selectedGrade.Rare == OriginalRareType.SR)
            {
                int avg = charProbs.Count == 0 ? 100 : Math.Max(1, (int)charProbs.Values.Average(r => r.Prob));
                foreach (var kv in GetLimitedCandidates(user, OriginalRareType.SR, avg))
                    charProbs.TryAdd(kv.Key, kv.Value);
            }
        }

        int maxCharProb = 0;

        Dictionary<int, (int minProbInc, int maxProbEx)> charProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        foreach (GachaListProbRecord charProb in charProbs.Values)
        {
            charProbsTable.Add(charProb.Id, new(maxCharProb, maxCharProb + charProb.Prob));
            maxCharProb += charProb.Prob;
        }

        // Now, do the pull
        int charRoll = (int)random.NextInt64(maxCharProb);

        GachaListProbRecord selectedCharacter = charProbs[charProbsTable.Where(p => charRoll >= p.Value.minProbInc && charRoll < p.Value.maxProbEx).Select(p => p.Key).First()];


        // We need to check if this is a selectup gacha. The Gacha ID will be empty in this case and must be obtained from the user object.
        int characterID = -1;

        switch (selectedCharacter.GachaType)
        {
            case GachaCategory.GachaSelectup:
                try
                {
                    characterID = GameData.Instance.GachaSelectupListTable[user.GachaSelectupChoices[gachaType.Id]].CharacterId;
                }
                catch
                {
                    Logging.WriteLine("[SelectRandomCharacter] Could not get the character from the selectup choice");
                }
                break;

            default:
                characterID = selectedCharacter.GachaId;
                break;
        }

        // Return
        return GameData.Instance.CharacterTable[characterID]; // GachaId is the character ID

    }

    /// <summary>
    /// This functions is mostly used for the tutorial pull and will not handle the selectup banners (already handled by the main gacha pull function)
    /// </summary>
    /// <param name="selectedGrade"></param>
    /// <param name="wishlistCharacters">Wishlist is handled just in cas but will most likely always be empty</param>
    /// <param name="user"></param>
    /// <returns></returns>
    private static CharacterRecord SelectRandomCharacterFromProbList(GachaGradeProbRecord selectedGrade, List<CharacterRecord> wishlistCharacters, User user)
    {
        Dictionary<int, GachaListProbRecord> charProbs = new();

        // Process the wishlist here
        // We filter the character from the category by the ones in the wishlist.
        // CustomizeListId seem to indicate that a wishlist can be specified. It seem to be only different for the two SSR groups in standard banners.
        // Wishlist must not be empty and must have all slots filled in.
        if (wishlistCharacters != null && wishlistCharacters.Count == 20 && selectedGrade.GachaListId != selectedGrade.CustomizeListId && selectedGrade.CustomizeListId != 0)
        {
            Logging.WriteLine($"Using wishlisted characters.");
            int[] ids = wishlistCharacters.Select(c => c.Id).ToArray();
            charProbs = GameData.Instance.GachaListProb.Where(g => g.Value.GroupId == selectedGrade.GachaListId).Where(g => ids.Contains(g.Value.GachaId)).ToDictionary();
        }
        // Otherwise, proceed with regular gacha list
        else
        {
            Logging.WriteLine($"Not using wishlisted characters.{(wishlistCharacters == null || wishlistCharacters.Count != 20 ? " Invalid wishlist." : "")}{(selectedGrade.GachaListId == selectedGrade.CustomizeListId || selectedGrade.CustomizeListId == 0 ? " Invalid Banner or Grade." : "")}");
            charProbs = GameData.Instance.GachaListProb.Where(g => g.Value.GroupId == selectedGrade.GachaListId).ToDictionary();
        }

        int maxCharProb = 0;

        Dictionary<int, (int minProbInc, int maxProbEx)> charProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        foreach (GachaListProbRecord charProb in charProbs.Values)
        {
            charProbsTable.Add(charProb.Id, new(maxCharProb, maxCharProb + charProb.Prob));
            maxCharProb += charProb.Prob;
        }

        // Now, do the pull
        int charRoll = (int)random.NextInt64(maxCharProb);

        GachaListProbRecord selectedCharacter = charProbs[charProbsTable.Where(p => charRoll >= p.Value.minProbInc && charRoll < p.Value.maxProbEx).Select(p => p.Key).First()];

        // Return
        return GameData.Instance.CharacterTable[selectedCharacter.GachaId];
    }

}