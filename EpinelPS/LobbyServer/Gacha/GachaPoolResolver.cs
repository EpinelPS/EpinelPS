using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

internal static class GachaPoolResolver
{
    private static readonly Random Random = new();

    public static GachaSelectupListRecord_Raw? GetSelectedOrDefaultSelectup(User user, int gachaTypeId)
    {
        if (user.GachaSelectupSelections.TryGetValue(gachaTypeId, out int selectedId) &&
            GameData.Instance.GachaSelectupList.TryGetValue(selectedId, out GachaSelectupListRecord_Raw? selected) &&
            selected.GachaTypeId == gachaTypeId)
        {
            return selected;
        }

        return GameData.Instance.GachaSelectupList.Values
            .Where(selectup => selectup.GachaTypeId == gachaTypeId)
            .OrderByDescending(selectup => selectup.IsDefault)
            .ThenBy(selectup => selectup.Order)
            .FirstOrDefault();
    }

    public static int ResolveFeaturedCharacterId(int requestTid, User user)
    {
        return ResolveContext(requestTid, user)?.FeaturedCharacterId ?? 0;
    }

    public static CharacterRecord? SelectCharacter(int requestTid, User user, IReadOnlyCollection<int> exclusionList)
    {
        GachaPoolContext? context = ResolveContext(requestTid, user);
        if (context == null || context.GradeProbId == 0)
        {
            return null;
        }

        List<GachaGradeProbRecord> gradeRows = GameData.Instance.GachaGradeProb.Values
            .Where(row => row.GroupId == context.GradeProbId && row.Prob > 0)
            .ToList();

        if (gradeRows.Count == 0)
        {
            Logging.WriteLine($"Gacha pool requestTid={requestTid} has no grade rows for gradeProb={context.GradeProbId}", LogType.Warning);
            return null;
        }

        for (int attempt = 0; attempt < 8; attempt++)
        {
            GachaGradeProbRecord grade = SelectWeighted(gradeRows, row => row.Prob);
            CharacterRecord? character = SelectCharacterFromGrade(context, grade, exclusionList);
            if (character != null)
            {
                return character;
            }
        }

        Logging.WriteLine(
            $"Gacha pool requestTid={requestTid} gachaType={context.GachaType.Id} could not select a character from static tables",
            LogType.Warning);
        return null;
    }

    private static CharacterRecord? SelectCharacterFromGrade(
        GachaPoolContext context,
        GachaGradeProbRecord grade,
        IReadOnlyCollection<int> exclusionList)
    {
        Dictionary<int, WeightedCharacterCandidate> candidates = [];

        AddCandidates(candidates, grade.GachaListId, context, grade.Rare, exclusionList);

        List<WeightedCharacterCandidate> candidateList = candidates.Values
            .Where(candidate => candidate.Weight > 0)
            .ToList();

        if (candidateList.Count == 0)
        {
            return null;
        }

        return SelectWeighted(candidateList, candidate => candidate.Weight).Character;
    }

    private static void AddCandidates(
        Dictionary<int, WeightedCharacterCandidate> candidates,
        int listId,
        GachaPoolContext context,
        OriginalRareType rare,
        IReadOnlyCollection<int> exclusionList)
    {
        if (listId == 0)
        {
            return;
        }

        foreach (GachaListProbRecord prob in GameData.Instance.GachaListProb.Values.Where(row => row.GroupId == listId && row.Prob > 0))
        {
            CharacterRecord? character = ResolveCandidateCharacter(prob, context);
            if (character == null || !IsUsableCharacter(character, rare, exclusionList, context.FeaturedCharacterId))
            {
                continue;
            }

            if (!candidates.TryGetValue(character.Id, out WeightedCharacterCandidate? existing) || prob.Prob > existing.Weight)
            {
                candidates[character.Id] = new WeightedCharacterCandidate(character, prob.Prob);
            }
        }
    }

    private static CharacterRecord? ResolveCandidateCharacter(GachaListProbRecord prob, GachaPoolContext context)
    {
        if (prob.GachaId != 0)
        {
            return GameData.Instance.CharacterTable.TryGetValue(prob.GachaId, out CharacterRecord? character)
                ? character
                : null;
        }

        if (context.FeaturedCharacterId == 0 ||
            prob.GachaType != GachaCategory.GachaSelectup ||
            prob.GachaSubType != GachaSubType.PickupCharacter)
        {
            return null;
        }

        return GameData.Instance.CharacterTable.TryGetValue(context.FeaturedCharacterId, out CharacterRecord? featured)
            ? featured
            : null;
    }

    private static bool IsUsableCharacter(
        CharacterRecord character,
        OriginalRareType rare,
        IReadOnlyCollection<int> exclusionList,
        int featuredCharacterId)
    {
        if (character.OriginalRare != rare)
        {
            return false;
        }

        if (exclusionList.Contains(character.Id) && character.Id != featuredCharacterId)
        {
            return false;
        }

        return character.GradeCoreId == 1 ||
            character.GradeCoreId == 101 ||
            character.GradeCoreId == 201 ||
            character.NameCode == 3999;
    }

    private static GachaPoolContext? ResolveContext(int requestTid, User user)
    {
        if (GameData.Instance.gachaTypes.TryGetValue(requestTid, out GachaTypeRecord? gachaType))
        {
            return BuildContext(gachaType, user, null);
        }

        if (GameData.Instance.GachaSelectupList.TryGetValue(requestTid, out GachaSelectupListRecord_Raw? selectup) &&
            GameData.Instance.gachaTypes.TryGetValue(selectup.GachaTypeId, out gachaType))
        {
            return BuildContext(gachaType, user, selectup);
        }

        GachaTypeRecord? pickupType = GameData.Instance.gachaTypes.Values
            .Where(gacha => gacha.PickupCharGroupId != 0)
            .FirstOrDefault(gacha => GameData.Instance.GachaListProb.Values
                .Any(prob => prob.GroupId == gacha.PickupCharGroupId && prob.GachaId == requestTid));

        if (pickupType != null)
        {
            return BuildContext(pickupType, user, null, requestTid);
        }

        return null;
    }

    private static GachaPoolContext BuildContext(
        GachaTypeRecord gachaType,
        User user,
        GachaSelectupListRecord_Raw? explicitSelectup,
        int explicitFeaturedCharacterId = 0)
    {
        GachaSelectupListRecord_Raw? selectup = explicitSelectup;
        if (selectup == null && gachaType.Type == GachaPremiumType.GachaSelectup)
        {
            selectup = GetSelectedOrDefaultSelectup(user, gachaType.Id);
        }

        int gradeProbId = selectup?.GachaGradeProbId > 0 ? selectup.GachaGradeProbId : gachaType.GradeProbId;
        int featuredListId = selectup?.GachaListId > 0 ? selectup.GachaListId : gachaType.PickupCharGroupId;
        int featuredCharacterId = explicitFeaturedCharacterId != 0
            ? explicitFeaturedCharacterId
            : selectup?.CharacterId ?? ResolvePickupCharacterId(gachaType);

        return new GachaPoolContext(gachaType, selectup, gradeProbId, featuredListId, featuredCharacterId);
    }

    private static int ResolvePickupCharacterId(GachaTypeRecord gachaType)
    {
        if (gachaType.PickupCharGroupId == 0)
        {
            return 0;
        }

        return GameData.Instance.GachaListProb.Values
            .Where(prob => prob.GroupId == gachaType.PickupCharGroupId)
            .Where(prob => prob.GachaSubType == GachaSubType.PickupCharacter || prob.GachaSubType == GachaSubType.LimitedCharacter)
            .OrderByDescending(prob => prob.Prob)
            .ThenBy(prob => prob.Id)
            .Select(prob => prob.GachaId)
            .FirstOrDefault();
    }

    private static T SelectWeighted<T>(IReadOnlyList<T> items, Func<T, int> getWeight)
    {
        long totalWeight = items.Sum(item => Math.Max(0, getWeight(item)));
        if (totalWeight <= 0)
        {
            return items[Random.Next(items.Count)];
        }

        long roll = Random.NextInt64(totalWeight);
        long running = 0;
        foreach (T item in items)
        {
            running += Math.Max(0, getWeight(item));
            if (roll < running)
            {
                return item;
            }
        }

        return items[^1];
    }

    private sealed record GachaPoolContext(
        GachaTypeRecord GachaType,
        GachaSelectupListRecord_Raw? Selectup,
        int GradeProbId,
        int FeaturedListId,
        int FeaturedCharacterId);

    private sealed record WeightedCharacterCandidate(CharacterRecord Character, int Weight);
}
