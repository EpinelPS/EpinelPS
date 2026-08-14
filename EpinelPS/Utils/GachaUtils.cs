
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
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

        if (user.sickpulls) // Left this in for backward compatibility with previous code.
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
                Logging.WriteLine("[SelectRandomCharacter] Tutorial Gacha Banners must have 10 pulls",LogType.Error);
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


    private static CharacterRecord SelectRandomCharacter(GachaTypeRecord gachaType, List<CharacterRecord> wishlistCharacters, User user)
    {

        // Load the categories and they probabilities
        // This table holds a link to premade lists for each grade and banner. No need to check each character type individually
        Dictionary<int, GachaGradeProbRecord> gradeProbs = GameData.Instance.GachaGradeProb.Where(p => p.Value.GroupId == gachaType.GradeProbId).OrderBy(p => p.Value.Prob).ToDictionary();

        // Build a probability table for the grades
        int maxProb = gradeProbs.Sum(p => p.Value.Prob);

        Dictionary<int, (int minProbInc, int maxProbEx)> gradeProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        int curVal = 0;

        foreach (GachaGradeProbRecord gradeProb in gradeProbs.Values)
        {
            gradeProbsTable.Add(gradeProb.Id, new(curVal, curVal + gradeProb.Prob));
            curVal += gradeProb.Prob;
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
                catch {
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
