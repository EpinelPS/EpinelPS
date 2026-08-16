using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

//
// This endpoint was adapted from ExecGacha
//

[GameRequest("/gacha/pity/execute")]
public class ExecutePityGacha : LobbyMessage
{
    protected override async Task HandleAsync()
    {

        ReqExecuteGachaPity req = await ReadData<ReqExecuteGachaPity>();

        ResExecuteGachaPity response = new ResExecuteGachaPity();
        response.Reward = new NetRewardData();

        // Add the pity count
        User user = GetUser();
        user.AddPityExecuteCount(req.GachaPityId);


        // Obtain the character
        List<CharacterRecord> selectedCharacters = new List<CharacterRecord>();
        selectedCharacters.Add(GameData.Instance.CharacterTable[req.CharacterId]);

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

                response.Reward.UserCharacters.Add(new NetUserCharacterDefaultData()
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
                    response.Reward.Character.Add(new NetCharacterData()
                    {
                        Csn = gacha.Sn,
                        CurrencyValue = gacha.CurrencyValue,
                        PieceCount = gacha.PieceCount,
                        Tid = characterData.Id
                    });

                    gacha.PieceCount = 1;
                    if (existingItem != null)
                    {
                        existingItem.Count++;

                        // Send the updated item in the response
                        response.Reward.UserItems.Add(new NetUserItemData()
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
                        response.Reward.UserItems.Add(new NetUserItemData()
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
                response.Reward.UserCharacters.Add(new NetUserCharacterDefaultData()
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

                response.Reward.Character.Add(new NetCharacterData()
                {
                    Csn = gacha.Sn,
                    CurrencyValue = gacha.CurrencyValue,
                    PieceCount = gacha.PieceCount,
                    Tid = characterData.Id
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

        }

        if (totalBodyLabels != 0)
        {
            if (totalBodyLabels < 0)
                user.SubtractCurrency(CurrencyType.DissolutionPoint, -totalBodyLabels);
            else
                user.AddCurrency(CurrencyType.DissolutionPoint, totalBodyLabels);

            response.Reward.Currency.Add(new NetCurrencyData
            {
                Type = (int)CurrencyType.DissolutionPoint,
                Value = totalBodyLabels,
                FinalValue = user.GetCurrencyVal(CurrencyType.DissolutionPoint)
            });
        }

        var pityBanner = GameData.Instance.GachaPityRecords.First().Value;
        response.GachaPityProgress = new NetGachaPityData()
        {
            GachaPityId = req.GachaPityId,
            PityExecuteCount = user.GachaPityBannerExecuteCount.Where(b => b.Key == req.GachaPityId).Sum(b => b.Value),
            TotalGachaCount = user.GetGachaCountForType(GachaPremiumType.GachaPremium),
            TotalUsedGachaCount = user.GachaPityBannerExecuteCount.Where(b => b.Key == req.GachaPityId).Sum(b => b.Value) * pityBanner.NeedGachaCount
        };



        JsonDb.Save();

        await WriteDataAsync(response);
    }
}