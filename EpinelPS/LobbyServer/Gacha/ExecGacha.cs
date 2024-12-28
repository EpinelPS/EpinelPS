//todo
//implement response.Reward 
// and response.Currencies
//NetUserCurrencyData fields Type 9000 and Value 150
//NetRewardData field Currency = new NetUserCurrencyData copy type and value from response.Currencies new NetUserCurrencyData
using EpinelPS.Database;
using EpinelPS.StaticInfo;
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
            var req = await ReadData<ReqExecuteGacha>();

            // Count determines whether we select 1 or 10 characters
            int numberOfPulls = req.Count == 1 ? 1 : 10;

            var user = GetUser();
            var response = new ResExecuteGacha() { Reward = new NetRewardData() { PassPoint = new() } };

            var entireallCharacterData = GameData.Instance.characterTable.Values.ToList();
            // Remove the .Values part since it's already a list.
            // Group by name_code to treat same name_code as one character 
            // Always add characters with grade_core_id == 1 and 101
            var allCharacterData = entireallCharacterData.GroupBy(c => c.name_code).SelectMany(g => g.Where(c => c.grade_core_id == 1 || c.grade_core_id == 101 || c.grade_core_id == 201 || c.name_code == 3999)).ToList();

            // Separate characters by rarity categories
            var rCharacters = allCharacterData.Where(c => c.original_rare == "R").ToList();
            var srCharacters = allCharacterData.Where(c => c.original_rare == "SR").ToList();

            // Separate Pilgrim SSRs and non-Pilgrim SSRs
            var pilgrimCharacters = allCharacterData.Where(c => c.original_rare == "SSR" && c.corporation == "PILGRIM").ToList();
            var ssrCharacters = allCharacterData.Where(c => c.original_rare == "SSR" && c.corporation != "PILGRIM").ToList();

            var selectedCharacters = new List<CharacterRecord>();

            // Check if user has 'sickpulls' set to true to use old method
            if (user.sickpulls)
            {
                // Old selection method: Randomly select characters based on req.Count value, excluding characters in the sickPullsExclusionList
                selectedCharacters = allCharacterData.Where(c => !sickPullsExclusionList.Contains(c.id)).OrderBy(x => random.Next()).Take(numberOfPulls).ToList(); // Exclude characters based on the exclusion list for sick pulls
            }
            else
            {
                // New method: Select characters based on req.Count value, with each character having its category determined independently, excluding characters in the normalPullsExclusionList
                for (int i = 0; i < numberOfPulls; i++)
                {
                    var character = SelectRandomCharacter(rCharacters, srCharacters, ssrCharacters, pilgrimCharacters, normalPullsExclusionList);
                    selectedCharacters.Add(character);
                }
            }

            int totalBodyLabels = 0;

            foreach (var characterData in selectedCharacters)
            {
                var gacha = new NetGachaEntityData()
                {
                    // PieceCount = 1, // Spare Body
                    CurrencyValue = 0, // Body Label
                    Tid = characterData.id,
                    Type = 1
                };


                // Check if user has this character.
                // If so, check if it is fully limit broken, then add body labels
                // If not fully limit broken, add spare body item
                // If user does not have character, generate CSN and add character

                if (user.HasCharacter(characterData.id))
                {
                    Database.Character character = user.GetCharacter(characterData.id) ?? throw new Exception("HasCharacter() returned true, however character was null");

                    var existingItem = user.Items.FirstOrDefault(item => item.ItemType == characterData.piece_id);

                    response.Characters.Add(new NetUserCharacterDefaultData()
                    {
                        CostumeId = character.CostumeId,
                        Csn = character.Csn,
                        Grade = character.Grade,
                        Level = character.Level,
                        UltiSkillLv = character.UltimateLevel,
                        Skill1Lv = character.Skill1Lvl,
                        Skill2Lv = character.Skill2Lvl,
                        Tid = characterData.id,
                    });

                    bool increase_item = false;

                    gacha.Sn = character.Csn;
                    gacha.Tid = characterData.id;

                    // Check if we can add upgrade item
                    if (characterData.original_rare == "SR")
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
                    else if (characterData.original_rare == "SSR")
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
                                Level = existingItem.Level,
                                Exp = existingItem.Exp,
                                Position = existingItem.Position,
                                Isn = existingItem.Isn
                            });
                        }
                        else
                        {
                            // If the item does not exist, create a new item entry
                            var newItem = new ItemData()
                            {
                                ItemType = characterData.piece_id,
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
                                Level = newItem.Level,
                                Exp = newItem.Exp,
                                Position = newItem.Position,
                                Isn = newItem.Isn
                            });
                        }
                    }
                    else
                    {
                        gacha.CurrencyValue = characterData.original_rare == "SSR" ? 6000 : (characterData.original_rare == "SR" ? 200 : 150);
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
                        Level = 1,
                        Skill1Lv = 1,
                        Skill2Lv = 1,
                        Tid = characterData.id,
                        UltiSkillLv = 1
                    });

                    user.Characters.Add(new Database.Character()
                    {
                        CostumeId = 0,
                        Csn = (int)gacha.Sn,
                        Grade = 0,
                        Level = 1,
                        Skill1Lvl = 1,
                        Skill2Lvl = 1,
                        Tid = characterData.id,
                        UltimateLevel = 1
                    });

                    // Add "New Character" Badge
                    user.AddBadge(BadgeContents.BadgeContentsNikkeNew, characterData.name_code.ToString());

                    if (characterData.original_rare == "SSR" || characterData.original_rare == "SR")
                    {
                        user.BondInfo.Add(new() { NameCode = characterData.name_code, Level = 1 });

                        if (characterData.original_rare == "SSR")
                        {
                            user.AddTrigger(TriggerType.ObtainCharacterSSR, characterData.name_code);
                        }
                        else
                        {
                            user.AddTrigger(TriggerType.ObtainCharacter, characterData.name_code);
                        }
                    }
                }

                response.Gacha.Add(gacha);

                user.AddTrigger(TriggerType.GachaCharacter, 0, 0);
            }
			var TicketType = req.CurrencyType;
			var currencyType = TicketType == 5100 ? (int)CurrencyType.SilverMileageTicket : (int)CurrencyType.GoldMileageTicket;
			var currencyTypeToAdd = TicketType == 5100 ? CurrencyType.SilverMileageTicket : CurrencyType.GoldMileageTicket;
			response.Reward.Currency.Add(new NetCurrencyData() { Type = currencyType, Value = numberOfPulls });
            response.Reward.Currency.Add(new NetCurrencyData() { Type = (int)CurrencyType.DissolutionPoint, Value = totalBodyLabels });
            user.AddCurrency(currencyTypeToAdd, numberOfPulls);

            user.GachaTutorialPlayCount++;

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private static CharacterRecord SelectRandomCharacter(List<CharacterRecord> rCharacters, List<CharacterRecord> srCharacters, List<CharacterRecord> ssrCharacters, List<CharacterRecord> pilgrimCharacters, List<int> exclusionList)
        {
            // Remove excluded characters from each category
            var availableRCharacters = rCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();
            var availableSRCharacters = srCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();
            var availableSSRCharacters = ssrCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();
            var availablePilgrimCharacters = pilgrimCharacters.Where(c => !exclusionList.Contains(c.id)).ToList();

            // Each time we call this method, a new category will be selected for a single character
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