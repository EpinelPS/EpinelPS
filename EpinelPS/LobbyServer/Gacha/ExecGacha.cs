//todo
//implement response.Reward 
// and response.Currencies
//NetUserCurrencyData fields Type 9000 and Value 150
//NetRewardData field Currency = new NetUserCurrencyData copy type and value from response.Currencies new NetUserCurrencyData


// Gacha backend data structure:
//
//  GachaTypeTable (Id = tid passed when called ExecGacha) - Identifies the banner calling the gacha process (standard, social, specific character, etc.)
//      GachaGradeProbTable (GroupId = GachaTypeTable.GradeProbId) - Lists 4 groups with probabilities (R, SR, SSR, Pilgrim/Overspecs)
//          GachaListProbTable (GroupId = GachaGradeProbTable.GachaListId) - Lists individual character per group
//



using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/execute")]
public class ExecGacha : LobbyMessage
{
    private static readonly Random random = new();

    // Some magic numbers for convenience
    private const int STANDARD_BANNER_ID = 1;
    private const int SOCIAL_BANNER_ID = 2;
    private const int NEW_PLAYER_SPECIAL_BANNER_ID = 4;

    // Exclusion lists for sick pulls mode and normal mode 2500601 is the broken R rarity dorothy
    private static readonly List<int> sickPullsExclusionList = [2500601]; // Add more IDs as needed
    private static readonly List<int> normalPullsExclusionList = [2500601, 422401, 306201, 399901, 399902, 399903, 399904, 201401, 301501, 112101, 313201, 319301, 319401, 320301, 422601, 426101, 328301, 328401, 235101, 235301, 136101, 339201, 140001, 140101, 140201, 580001, 580101, 580201, 581001, 581101, 581201, 582001, 582101, 582201, 583001, 583101, 583201, 583301, 190101, 290701]; // Add more IDs as needed

    protected override async Task HandleAsync()
    {
        // Get the request data
        ReqExecuteGacha req = await ReadData<ReqExecuteGacha>();

        // Count determines whether we select 1 or 10 characters
        int numberOfPulls = req.Count == 1 ? 1 : 10;

        //Get the banner ID and load the banner data from GachaTypeTable
        int bannerID =  req.Tid; 
        Logging.WriteLine($"Banner ID: {bannerID}");  
        GachaTypeRecord gachaType = GameData.Instance.gachaTypes[bannerID];      
          
        // Get the user so that we can check for wishlisted characters (if needed)
        User user = GetUser();

        ResExecuteGacha response = new() { Reward = new NetRewardData() { PassPoint = new() } };

        List<CharacterRecord> entireallCharacterData = [.. GameData.Instance.CharacterTable.Values];
        Logging.WriteLine($"Currency type: {(CurrencyType)req.CurrencyType}");

        // Fill the wishlist anyway. It won't be used of not compatible with the banner.
        List<CharacterRecord> wishlistCharacters = [.. entireallCharacterData.Where(c => user.CharacterWishlist.Contains(c.Id))];
               
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
        else
        {
            // Now using the game probability tables to do the pulls
            for (int i = 0; i < numberOfPulls; i++)
            {
                // Added bannerID to the SelectRandomCharacter method to allow for specific logic such as wishlist handling
                CharacterRecord character = SelectRandomCharacter(gachaType, wishlistCharacters);
                selectedCharacters.Add(character);
            }
        }
        int totalBodyLabels = 0;

        foreach (CharacterRecord characterData in selectedCharacters)
        {
            NetGachaEntityData gacha = new()
            {
                // PieceCount = 1, // Spare Body
                CurrencyValue = 0, // Body Label
                Tid = characterData.Id,
                Type = 1
            };


            // Check if user has this character.
            // If so, check if it is fully limit broken, then add body labels
            // If not fully limit broken, add spare body item
            // If user does not have character, generate CSN and add character

            if (user.HasCharacter(characterData.Id))
            {
                CharacterModel character = user.GetCharacter(characterData.Id) ?? throw new Exception("HasCharacter() returned true, however character was null");

                DbItemData? existingItem = user.Items.FirstOrDefault(item => item.ItemType == characterData.PieceId);

                response.Characters.Add(new NetUserCharacterDefaultData()
                {
                    CostumeId = character.CostumeId,
                    Csn = character.Csn,
                    Grade = character.Grade,
                    Lv = character.Level,
                    UltiSkillLv = character.UltimateLevel,
                    Skill1Lv = character.Skill1Lvl,
                    Skill2Lv = character.Skill2Lvl,
                    Tid = characterData.Id,
                });

                bool increase_item = false;

                gacha.Sn = character.Csn;
                gacha.Tid = characterData.Id;

                // Check if we can add upgrade item
                if (characterData.OriginalRare == OriginalRareType.SR)
                {
                    if (existingItem != null && character.Grade + existingItem.Count <= 1)
                    {
                        increase_item = true;
                    }
                    else if (existingItem == null && character.Grade <= 1)
                    {
                        increase_item = true;
                    }
                }
                else if (characterData.OriginalRare == OriginalRareType.SSR)
                {
                    if (existingItem != null && character.Grade + existingItem.Count <= 10)
                    {
                        increase_item = true;
                    }
                    else if (existingItem == null && character.Grade <= 10)
                    {
                        increase_item = true;
                    }
                }

                if (increase_item)
                {
                    gacha.PieceCount = 1;
                    if (existingItem != null)
                    {
                        existingItem.Count++;

                        // Send the updated item in the response
                        response.Items.Add(new NetUserItemData()
                        {
                            Tid = existingItem.ItemType,
                            Csn = existingItem.Csn,
                            Count = existingItem.Count,
                            Lv = existingItem.Level,
                            Exp = existingItem.Exp,
                            Position = existingItem.Position,
                            Isn = existingItem.Isn
                        });
                    }
                    else
                    {
                        // If the item does not exist, create a new item entry
                        DbItemData newItem = new()
                        {
                            ItemType = characterData.PieceId,
                            Csn = 0,
                            Count = 1, // or any relevant count
                            Level = 0,
                            Exp = 0,
                            Position = 0,
                            Corp = 0,
                            Isn = user.GenerateUniqueItemId()
                        };
                        user.Items.Add(newItem);

                        // Add the new item to response
                        response.Items.Add(new NetUserItemData()
                        {
                            Tid = newItem.ItemType,
                            Csn = newItem.Csn,
                            Count = newItem.Count,
                            Lv = newItem.Level,
                            Exp = newItem.Exp,
                            Position = newItem.Position,
                            Isn = newItem.Isn
                        });
                    }
                }
                else
                {
                    gacha.CurrencyValue = characterData.OriginalRare == OriginalRareType.SSR ? 6000 : (characterData.OriginalRare == OriginalRareType.SR ? 200 : 150);
                    user.AddCurrency(CurrencyType.DissolutionPoint, gacha.CurrencyValue);

                    totalBodyLabels += (int)gacha.CurrencyValue;
                }
            }
            else
            {
                // Add new character to user
                gacha.Sn = user.GenerateUniqueCharacterId();
                response.Characters.Add(new NetUserCharacterDefaultData()
                {
                    CostumeId = 0,
                    Csn = gacha.Sn,
                    Grade = 0,
                    Lv = 1,
                    Skill1Lv = 1,
                    Skill2Lv = 1,
                    Tid = characterData.Id,
                    UltiSkillLv = 1
                });

                user.Characters.Add(new CharacterModel()
                {
                    CostumeId = 0,
                    Csn = (int)gacha.Sn,
                    Grade = 0,
                    Level = 1,
                    Skill1Lvl = 1,
                    Skill2Lvl = 1,
                    Tid = characterData.Id,
                    UltimateLevel = 1
                });

                // Add "New Character" Badge
                user.AddBadge(BadgeContents.NikkeNew, characterData.NameCode.ToString());
                user.AddTrigger(Trigger.ObtainCharacter, 1, characterData.NameCode);
                user.AddTrigger(Trigger.ObtainCharacterNew, 1);

                if (characterData.OriginalRare == OriginalRareType.SSR || characterData.OriginalRare == OriginalRareType.SR)
                {
                    user.BondInfo.Add(new() { NameCode = characterData.NameCode, Lv = 1 });

                }
            }

            response.Gacha.Add(gacha);

            user.AddTrigger(Trigger.GachaCharacter, 0, 0);
        }

        CurrencyType ticketType = (CurrencyType)req.CurrencyType;

        // ==========================
        // CAPABILITIES (single source of truth)
        // ==========================
        bool canUsePremiumTicket = ticketType == CurrencyType.CharPremiumTicket;
        bool canUseCustomizeTicket = ticketType == CurrencyType.CharCustomizeTicket;
        bool canUseFreeCash =
            ticketType == CurrencyType.CharPremiumTicket ||
            ticketType == CurrencyType.CharCustomizeTicket ||
            ticketType == CurrencyType.FreeCash;

        // ==========================
        // STATE
        // ==========================
        long pullsLeft = numberOfPulls;
        bool discount = req.IsDiscount;

        long usePremiumTickets = 0;
        long useCharCustomizeTickets = 0;
        long useFreeCash = 0;
        long useChargeCash = 0;
        long useFriendshipPoint = 0;

        long userPremiumTickets = user.GetCurrencyVal(CurrencyType.CharPremiumTicket);
        long userCharCustomizeTickets = user.GetCurrencyVal(CurrencyType.CharCustomizeTicket);
        long userFreeCash = user.GetCurrencyVal(CurrencyType.FreeCash);
        // ==========================
        // EXCLUSIVE CURRENCIES
        // ==========================
        switch (ticketType)
        {
            case CurrencyType.ChargeCash:{
                    var cashPrice = gachaType.GachaPriceGroup.Where(g => g.GachaPriceType == 98 /*CurrencyType.ChargeCash*/ || g.GachaPriceType == 99 /*CurrencyType.FreeCash*/).First();
                    useChargeCash = pullsLeft * (discount ?  cashPrice.DailyGachaDiscountPriceValue1 : cashPrice.GachaPriceValueCount1);
                    pullsLeft = 0;
                }
                break;

            case CurrencyType.FriendshipPoint:{
                    var fpPrice = gachaType.GachaPriceGroup.Where(g => g.GachaPriceType == 4000 /*CurrencyType.FriendshipPoint*/).First();
                    useFriendshipPoint = pullsLeft * fpPrice.GachaPriceValueCount1;
                    pullsLeft = 0;
                }
                break;
        }

        // ==========================
        // MIXED PAYMENT PIPELINE
        // ==========================
        if (canUsePremiumTicket)
        {
            usePremiumTickets = Math.Min(userPremiumTickets, pullsLeft);
            pullsLeft -= usePremiumTickets;
        }

        if (canUseCustomizeTicket)
        {
            useCharCustomizeTickets = Math.Min(userCharCustomizeTickets, pullsLeft);
            pullsLeft -= useCharCustomizeTickets;
        }

        if (canUseFreeCash)
        {
            var cashPrice = gachaType.GachaPriceGroup.Where(g => g.GachaPriceType == 98 /*CurrencyType.ChargeCash*/ || g.GachaPriceType == 99 /*CurrencyType.FreeCash*/).First();
            long costPerPull = discount ? cashPrice.DailyGachaDiscountPriceValue1 : cashPrice.GachaPriceValueCount1;
            long totalCostNeeded = pullsLeft * costPerPull;

            useFreeCash = Math.Min(userFreeCash, totalCostNeeded);
            useChargeCash = totalCostNeeded - useFreeCash;

            pullsLeft = 0;
        }

        // ==========================
        // APPLY CURRENCY CHANGES
        // ==========================
        void ApplyCurrency(CurrencyType type, long delta)
        {
            if (delta == 0) return;

            if (delta < 0)
                user.SubtractCurrency(type, -delta);
            else
                user.AddCurrency(type, delta);

            response.Currencies.Add(new NetUserCurrencyData
            {
                Type = (int)type,
                Value = user.GetCurrencyVal(type)
            });
        }

        ApplyCurrency(CurrencyType.CharPremiumTicket, -usePremiumTickets);
        ApplyCurrency(CurrencyType.CharCustomizeTicket, -useCharCustomizeTickets);
        ApplyCurrency(CurrencyType.FreeCash, -useFreeCash);
        ApplyCurrency(CurrencyType.ChargeCash, -useChargeCash);
        ApplyCurrency(CurrencyType.FriendshipPoint, -useFriendshipPoint);
        ApplyCurrency(CurrencyType.DissolutionPoint, totalBodyLabels);

        // ==========================
        // MILEAGE REWARDS
        // ==========================
        if (bannerID == STANDARD_BANNER_ID)            
            ApplyCurrency(CurrencyType.SilverMileageTicket, numberOfPulls);

        if (bannerID != STANDARD_BANNER_ID && bannerID != SOCIAL_BANNER_ID && bannerID != NEW_PLAYER_SPECIAL_BANNER_ID) // TODO: Handle daily free pulls. They should not give Gold Mileage.
            ApplyCurrency(CurrencyType.GoldMileageTicket, numberOfPulls);

        user.GachaTutorialPlayCount++;

        JsonDb.Save();

        await WriteDataAsync(response);
    }

    private static CharacterRecord SelectRandomCharacter(GachaTypeRecord gachaType, List<CharacterRecord> wishlistCharacters)
    {
       
        // Load the categories and they probabilities
        // This table holds a link to premade lists for each grade and banner. No need to check each character type individually
        Dictionary<int, GachaGradeProbRecord> gradeProbs = GameData.Instance.GachaGradeProb.Where(p => p.Value.GroupId == gachaType.GradeProbId).OrderBy(p => p.Value.Prob).ToDictionary(); 
       
        // Build a probability table for the grades
        int maxProb = gradeProbs.Sum(p => p.Value.Prob);

        Dictionary<int, (int minProbInc, int maxProbEx)> gradeProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        int curVal = 0;

        foreach(GachaGradeProbRecord gradeProb in gradeProbs.Values){
            gradeProbsTable.Add(gradeProb.Id, new (curVal, curVal + gradeProb.Prob));
            curVal += gradeProb.Prob;
        }
        
        // Now do the roll for the grades
        int gradeRoll = (int)random.NextInt64(maxProb);

        GachaGradeProbRecord selectedGrade = gradeProbs[gradeProbsTable.Where( p => gradeRoll >= p.Value.minProbInc && gradeRoll < p.Value.maxProbEx).Select( p=> p.Key).First()];

        // We have the grade, we need to pull the list of characters from GachaListProbTable and create a similar probability table
        // Prefered characters from special banner should be handled automatically by these lists
        // The maximum random value will change depending on how many characters are in the list so we need to keep track of it
        // selectedGrade.GachaListId != selectedGrade.CustomizeListId seem to indicate that wishlisting is supported by the banner. CustomizeListId 19995 and 19996 are most likely SSR and Pilgrims wishlists 

        Dictionary<int, GachaListProbRecord> charProbs = null;
        
        // Process the wishlist here
        // We filter the character from the category by the ones in the wishlist.
        // CustomizeListId seem to indicate that a wishlist can be specified. It seem to be only different for the two SSR groups in standard banners.
        // Wishlist must not be empty and must have all slots filled in.
        if (wishlistCharacters != null && wishlistCharacters.Count == 20 && selectedGrade.GachaListId != selectedGrade.CustomizeListId && selectedGrade.CustomizeListId != 0){
            int[] ids = wishlistCharacters.Select(c=> c.Id).ToArray();
            charProbs = GameData.Instance.GachaListProb.Where(g => g.Value.GroupId == selectedGrade.GachaListId).Where(g => ids.Contains(g.Value.GachaId) ).ToDictionary();
        }
        // Otherwise, proceed with regular gacha list
        else{                    
            charProbs = GameData.Instance.GachaListProb.Where(g => g.Value.GroupId == selectedGrade.GachaListId).ToDictionary();
        }

        int maxCharProb = 0;

        Dictionary<int, (int minProbInc, int maxProbEx)> charProbsTable = new Dictionary<int, (int minProbInc, int maxProbEx)>();

        foreach(GachaListProbRecord charProb in charProbs.Values){
            charProbsTable.Add(charProb.Id, new (maxCharProb, maxCharProb + charProb.Prob));
            maxCharProb += charProb.Prob;
        }

        // Now, do the pull
        int charRoll = (int)random.NextInt64(maxCharProb);

        GachaListProbRecord selectedCharacter = charProbs[charProbsTable.Where( p => charRoll >= p.Value.minProbInc && charRoll < p.Value.maxProbEx).Select( p=> p.Key).First()];

        // Return
        return GameData.Instance.CharacterTable[selectedCharacter.GachaId]; // GachaId is the character ID

    }

}