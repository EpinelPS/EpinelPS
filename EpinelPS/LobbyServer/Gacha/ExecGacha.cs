//todo
//implement response.Reward 
// and response.Currencies
//NetUserCurrencyData fields Type 9000 and Value 150
//NetRewardData field Currency = new NetUserCurrencyData copy type and value from response.Currencies new NetUserCurrencyData
using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha
{
    [PacketPath("/gacha/execute")]
    public class ExecGacha : LobbyMsgHandler
    {
        private static readonly Random random = new();

        // Exclusion lists for sick pulls mode and normal mode 2500601 is the broken R rarity dorothy
        private static readonly List<int> sickPullsExclusionList = [2500601]; // Add more IDs as needed
        private static readonly List<int> normalPullsExclusionList = [2500601, 422401, 306201, 399901, 399902, 399903, 399904, 201401, 301501, 112101, 313201, 319301, 319401, 320301, 422601, 426101, 328301, 328401, 235101, 235301, 136101, 339201, 140001, 140101, 140201, 580001, 580101, 580201, 581001, 581101, 581201, 582001, 582101, 582201, 583001, 583101, 583201, 583301, 190101, 290701]; // Add more IDs as needed

        protected override async Task HandleAsync()
        {
            ReqExecuteGacha req = await ReadData<ReqExecuteGacha>();
            int IncreasedChanceCharacterID = req.Tid;
			

            // Count determines whether we select 1 or 10 characters
            int numberOfPulls = req.Count == 1 ? 1 : 10;

            User user = GetUser();
            ResExecuteGacha response = new() { Reward = new NetRewardData() { PassPoint = new() } };

            List<CharacterRecord> entireallCharacterData = [.. GameData.Instance.CharacterTable.Values];
            // Remove the .Values part since it's already a list.
            // Group by NameCode to treat same NameCode as one character 
            // Always add characters with GradeCoreId == 1 and 101
            List<CharacterRecord> allCharacterData = [.. entireallCharacterData.GroupBy(c => c.NameCode).SelectMany(g => g.Where(c => c.GradeCoreId == 1 || c.GradeCoreId == 101 || c.GradeCoreId == 201 || c.NameCode == 3999))];

            // Separate characters by rarity categories
            List<CharacterRecord> rCharacters = [.. allCharacterData.Where(c => c.OriginalRare == OriginalRareType.R)];
            List<CharacterRecord> srCharacters = [.. allCharacterData.Where(c => c.OriginalRare == OriginalRareType.SR)];

            // Separate Pilgrim SSRs and non-Pilgrim SSRs
            // treat overspec as pilgrim 
            List<CharacterRecord> pilgrimCharacters = [.. allCharacterData.Where(c => (c.OriginalRare == OriginalRareType.SSR && c.Corporation == CorporationType.PILGRIM) || (c.OriginalRare == OriginalRareType.SSR && c.CorporationSubType == CorporationSubType.OVERSPEC))];
            List<CharacterRecord> ssrCharacters = [.. allCharacterData.Where(c => c.OriginalRare == OriginalRareType.SSR && c.Corporation != CorporationType.PILGRIM)];

            List<CharacterRecord> selectedCharacters = [];

            // Check if user has 'sickpulls' set to true to use old method
            if (user.sickpulls)
            {
                // Old selection method: Randomly select characters based on req.Count value, excluding characters in the sickPullsExclusionList
                selectedCharacters = [.. allCharacterData.Where(c => !sickPullsExclusionList.Contains(c.Id)).OrderBy(x => random.Next()).Take(numberOfPulls)]; // Exclude characters based on the exclusion list for sick pulls
            }
            else
            {
                // New method: Select characters based on req.Count value, with each character having its category determined independently, excluding characters in the normalPullsExclusionList
                for (int i = 0; i < numberOfPulls; i++)
                {
                    CharacterRecord character = SelectRandomCharacter(rCharacters, srCharacters, ssrCharacters, pilgrimCharacters, normalPullsExclusionList, IncreasedChanceCharacterID, allCharacterData);
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

                    ItemData? existingItem = user.Items.FirstOrDefault(item => item.ItemType == characterData.PieceId);

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
                            ItemData newItem = new()
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
            bool canUsePremiumTicket   = ticketType == CurrencyType.CharPremiumTicket;
            bool canUseCustomizeTicket = ticketType == CurrencyType.CharCustomizeTicket;
            bool canUseFreeCash        =
                ticketType == CurrencyType.CharPremiumTicket ||
                ticketType == CurrencyType.CharCustomizeTicket ||
                ticketType == CurrencyType.FreeCash;

            // ==========================
            // STATE
            // ==========================
            long pullsLeft = numberOfPulls;

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
                case CurrencyType.ChargeCash:
                    useChargeCash = pullsLeft * 200; // Note: ChargeCash discount pricing not implemented yet (UI currently shows 200).
                    pullsLeft = 0;
                    break;

                case CurrencyType.FriendshipPoint:
                    useFriendshipPoint = pullsLeft * 10;
                    pullsLeft = 0;
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
                long maxFreePulls = userFreeCash / 300;
                long freePullsUsed = Math.Min(maxFreePulls, pullsLeft);

                long costPerPull = 300; // Note: Free Cash / Charge Cash gacha cost discount not yet implemented

                if (numberOfPulls == 1)
                    costPerPull = 150; // Note: discount for single pull not yet implemented (UI currently shows 150).

                useFreeCash = freePullsUsed * costPerPull;
                pullsLeft -= freePullsUsed;

                if (pullsLeft > 0)
                {
                    useChargeCash = pullsLeft * costPerPull;
                    pullsLeft = 0;
                }
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
            if (ticketType == CurrencyType.CharPremiumTicket ||
                ticketType == CurrencyType.CharCustomizeTicket ||
                ticketType == CurrencyType.FreeCash ||
                ticketType == CurrencyType.FriendshipPoint)
                ApplyCurrency(CurrencyType.SilverMileageTicket, numberOfPulls);

            if (ticketType == CurrencyType.ChargeCash)
                ApplyCurrency(CurrencyType.GoldMileageTicket, numberOfPulls);

            user.GachaTutorialPlayCount++;

            JsonDb.Save();

            await WriteDataAsync(response);
        }

		private static CharacterRecord SelectRandomCharacter(List<CharacterRecord> rCharacters,List<CharacterRecord> srCharacters,List<CharacterRecord> ssrCharacters,List<CharacterRecord> pilgrimCharacters,List<int> exclusionList,int increasedChanceCharacterID,List<CharacterRecord> allCharacterData)
		{
            // Remove excluded characters from each category
            List<CharacterRecord> availableRCharacters = [.. rCharacters.Where(c => !exclusionList.Contains(c.Id))];
            List<CharacterRecord> availableSRCharacters = [.. srCharacters.Where(c => !exclusionList.Contains(c.Id))];
            List<CharacterRecord> availableSSRCharacters = [.. ssrCharacters.Where(c => !exclusionList.Contains(c.Id))];
            List<CharacterRecord> availablePilgrimCharacters = [.. pilgrimCharacters.Where(c => !exclusionList.Contains(c.Id))];

            // Find the IncreasedChanceCharacterID in the SSR list
            CharacterRecord? increasedChanceCharacter = availableSSRCharacters.FirstOrDefault(c => c.Id == increasedChanceCharacterID);
			bool isPilgrimOrOverspec = increasedChanceCharacter != null && (increasedChanceCharacter.Corporation == CorporationType.PILGRIM || increasedChanceCharacter.CorporationSubType == CorporationSubType.OVERSPEC);

			double increasedChance = increasedChanceCharacterID != 1 ? (isPilgrimOrOverspec ? 1.0 : 2.0): 0.0;

			double roll = random.NextDouble() * 100; // Roll from 0 to 100

			if (roll < 53 && availableRCharacters.Count != 0)
			{
				// R category
				return availableRCharacters[random.Next(availableRCharacters.Count)];
			}
			else if (roll < 53 + 43 && availableSRCharacters.Count != 0)
			{
				// SR category
				return availableSRCharacters[random.Next(availableSRCharacters.Count)];
			}
			else
			{
				// SSR category
				double ssrRoll = random.NextDouble() * 100;

				if (increasedChanceCharacter != null && ssrRoll < increasedChance)
				{
					// Increased Chance SSR
					return increasedChanceCharacter;
				}

				ssrRoll -= increasedChance;

				if (ssrRoll < 4.55 && availablePilgrimCharacters.Count != 0)
				{
					// PILGRIM SSR
					return availablePilgrimCharacters[random.Next(availablePilgrimCharacters.Count)];
				}
				else if (availableSSRCharacters.Count != 0)
				{
					// Non-PILGRIM SSR
					return availableSSRCharacters[random.Next(availableSSRCharacters.Count)];
				}
			}

			// Fallback to a random R character if somehow no SSR characters are left after exclusion
			return availableRCharacters.Count != 0 ? availableRCharacters[random.Next(availableRCharacters.Count)] : throw new Exception("cannot find any characters");
		}

    }
}