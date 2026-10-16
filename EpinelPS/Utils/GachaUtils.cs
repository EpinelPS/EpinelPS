
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.Utils;

public class GachaUtils
{
    private static readonly List<int> sickPullsExclusionList = [2500601]; // Add more IDs as needed
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, List<GachaGradeProbRecord>> _gradeProbsByGroup = new();
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, List<GachaListProbRecord>> _listProbsByGroup = new();

    public static List<CharacterRecord> ExecuteGachaPull(GachaTypeRecord gachaType, int numberOfPulls, User user)
    {
        List<CharacterRecord> wishlistCharacters = user.GetWishlistCharacters(gachaType.Id);
        List<CharacterRecord> selectedCharacters = [];

        if (user.sickpulls)
        {
            List<CharacterRecord> entireallCharacterData = [.. GameData.Instance.CharacterTable.Values];
            List<CharacterRecord> allCharacterData = [.. entireallCharacterData.GroupBy(c => c.NameCode).SelectMany(g => g.Where(c => (c.IsVisible && (c.GradeCoreId == 1 || c.GradeCoreId == 101 || c.GradeCoreId == 201)) || c.NameCode == 3999))];
            selectedCharacters = [.. allCharacterData.Where(c => !sickPullsExclusionList.Contains(c.Id)).OrderBy(_ => Random.Shared.Next()).Take(numberOfPulls)];
        }
        else if (gachaType.Type == GachaPremiumType.GachaTutorial)
        {
            if (numberOfPulls != 10)
            {
                Logging.WriteLine("[SelectRandomCharacter] Tutorial Gacha Banners must have 10 pulls", LogType.Error);
                throw new ArgumentException("Tutorial Gacha Banners must have 10 pulls");
            }

            long ssrSpot = Random.Shared.NextInt64(0, 10);

            for (int i = 0; i < numberOfPulls; i++)
            {
                if (i == ssrSpot)
                {
                    GachaGradeProbRecord gradeProbs = GameData.Instance.GachaGradeProb.Values
                        .First(p => p.GroupId == gachaType.GradeProbId && p.Rare == OriginalRareType.SSR);
                    CharacterRecord character = SelectRandomCharacterFromProbList(gradeProbs, wishlistCharacters, user);
                    selectedCharacters.Add(character);
                }
                else
                {
                    CharacterRecord character = SelectRandomCharacter(gachaType, wishlistCharacters, user);
                    selectedCharacters.Add(character);
                }
            }
        }
        else
        {
            for (int i = 0; i < numberOfPulls; i++)
            {
                CharacterRecord character = SelectRandomCharacter(gachaType, wishlistCharacters, user);
                selectedCharacters.Add(character);
            }
        }

        return selectedCharacters;
    }


    private static CharacterRecord SelectRandomCharacter(GachaTypeRecord gachaType, List<CharacterRecord> wishlistCharacters, User user)
    {
        var gradeProbs = _gradeProbsByGroup.GetOrAdd(gachaType.GradeProbId, id =>
            GameData.Instance.GachaGradeProb.Values
                .Where(p => p.GroupId == id)
                .OrderBy(p => p.Prob)
                .ToList());

        int maxProb = 0;
        foreach (var p in gradeProbs) maxProb += p.Prob;
        if (maxProb <= 0) maxProb = 1;

        int gradeRoll = (int)Random.Shared.NextInt64(maxProb);
        GachaGradeProbRecord selectedGrade = gradeProbs[0];
        int curVal = 0;
        foreach (var p in gradeProbs)
        {
            curVal += p.Prob;
            if (gradeRoll < curVal)
            {
                selectedGrade = p;
                break;
            }
        }

        return RollCharacterFromGrade(selectedGrade, wishlistCharacters, user, gachaType);
    }

    private static CharacterRecord RollCharacterFromGrade(GachaGradeProbRecord selectedGrade, List<CharacterRecord> wishlistCharacters, User user, GachaTypeRecord? gachaType)
    {
        var fullList = _listProbsByGroup.GetOrAdd(selectedGrade.GachaListId, id =>
            GameData.Instance.GachaListProb.Values
                .Where(g => g.GroupId == id)
                .ToList());

        List<GachaListProbRecord> candidateList;
        if (wishlistCharacters != null && wishlistCharacters.Count == 20 && selectedGrade.GachaListId != selectedGrade.CustomizeListId && selectedGrade.CustomizeListId != 0)
        {
            var ids = new HashSet<int>(wishlistCharacters.Select(c => c.Id));
            candidateList = fullList.Where(g => ids.Contains(g.GachaId)).ToList();
            if (candidateList.Count == 0) candidateList = fullList;
        }
        else
        {
            candidateList = fullList;
        }

        int maxCharProb = 0;
        foreach (var cp in candidateList) maxCharProb += cp.Prob;
        if (maxCharProb <= 0) maxCharProb = 1;

        int charRoll = (int)Random.Shared.NextInt64(maxCharProb);
        GachaListProbRecord selectedCharacter = candidateList[0];
        int accum = 0;
        foreach (var cp in candidateList)
        {
            accum += cp.Prob;
            if (charRoll < accum)
            {
                selectedCharacter = cp;
                break;
            }
        }

        int characterID = selectedCharacter.GachaId;
        if (selectedCharacter.GachaType == GachaCategory.GachaSelectup && gachaType != null)
        {
            try
            {
                if (user.GachaSelectupChoices.TryGetValue(gachaType.Id, out int choice))
                {
                    characterID = GameData.Instance.GachaSelectupListTable[choice].CharacterId;
                }
            }
            catch
            {
                Logging.WriteLine("[SelectRandomCharacter] Could not get the character from the selectup choice", LogType.Warning);
            }
        }

        return GameData.Instance.CharacterTable[characterID];
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
        return RollCharacterFromGrade(selectedGrade, wishlistCharacters, user, null);
    }

}
