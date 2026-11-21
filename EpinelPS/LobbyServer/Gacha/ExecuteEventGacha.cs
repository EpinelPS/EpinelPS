//yes i am lazy and its preety much same as exec gacha 
//but only does 1x pull
//its here only so there is no system error on 1x free gacha event

using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/event/execute")]
public class ExecuteEventGacha : LobbyMessage
{
    private static readonly Random random = new();

    // Exclusion lists for sick pulls mode and normal mode 2500601 is the broken R rarity dorothy
    private static readonly List<int> sickPullsExclusionList = [2500601]; // Add more IDs as needed
    private static readonly List<int> normalPullsExclusionList = [2500601, 422401, 306201, 399901, 399902, 399903, 399904, 201401, 301501, 112101, 313201, 319301, 319401, 320301, 422601, 426101, 328301, 328401, 235101, 235301, 136101, 339201, 140001, 140101, 140201, 580001, 580101, 580201, 581001, 581101, 581201, 582001, 582101, 582201, 583001, 583101, 583201, 583301, 190101, 290701]; // Add more IDs as needed

    protected override async Task HandleAsync()
    {
        ReqExecuteDailyFreeGacha req = await ReadData<ReqExecuteDailyFreeGacha>();

        // Daily free gacha is always a single pull
        const int numberOfPulls = 1;

        int bannerID = req.GachaId;
        Logging.WriteLine($"Event ID: {req.EventId}");
        Logging.WriteLine($"Banner ID: {bannerID}");
        GachaTypeRecord gachaType = GameData.Instance.gachaTypes[bannerID];

        User user = GetUser();

        ResExecuteDailyFreeGacha response = new();

        Logging.WriteLine($"Currency type: Daily free pull");

        List<CharacterRecord> selectedCharacters = GachaUtils.ExecuteGachaPull(gachaType, numberOfPulls, user);

        int totalBodyLabels = 0;

        foreach (CharacterRecord characterData in selectedCharacters)
        {
            // PieceCount = spare body, CurrencyValue = body label (dissolution point)
            NetGachaEntityData gacha = new()
            {
                CurrencyValue = 0,
                Tid = characterData.Id,
                Type = 1
            };

            // Limit break check, matching ExecGacha / UsePiece / EventShop:
            // Grade stores the limit break counter in its lower two digits (SR maxes out at 3,
            // SSR at 11). Fully limit broken characters and R characters never receive a spare
            // body, the pull is converted into dissolution points instead.
            bool givePiece = false;

            if (user.HasCharacter(characterData.Id))
            {
                CharacterModel owned = user.GetCharacter(characterData.Id)!;
                DbItemData? existingItem = user.Items.FirstOrDefault(item => item.ItemType == characterData.PieceId);

                // Spare bodies already in the inventory count towards the limit break progress,
                // otherwise a character at Grade 2 with 5 spare bodies would still receive more.
                int progress = (owned.Grade % 100) + (existingItem?.Count ?? 0);

                if (characterData.OriginalRare == OriginalRareType.SSR)
                    givePiece = progress < 11;
                else if (characterData.OriginalRare == OriginalRareType.SR)
                    givePiece = progress < 3;
                // R characters have no spare body item, so givePiece stays false for them.

                gacha.Sn = owned.Csn;

                response.Characters.Add(new NetUserCharacterDefaultData()
                {
                    CostumeId = owned.CostumeId,
                    Csn = owned.Csn,
                    Grade = owned.Grade,
                    Lv = owned.Level,
                    Skill1Lv = owned.Skill1Lvl,
                    Skill2Lv = owned.Skill2Lvl,
                    Tid = characterData.Id,
                    UltiSkillLv = owned.UltimateLevel,
                });

                if (givePiece)
                {
                    gacha.PieceCount = 1;

                    if (existingItem != null)
                    {
                        existingItem.Count++;

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
                        DbItemData newItem = new()
                        {
                            ItemType = characterData.PieceId,
                            Csn = 0,
                            Count = 1,
                            Level = 0,
                            Exp = 0,
                            Position = 0,
                            Corp = 0,
                            Isn = user.GenerateUniqueItemId()
                        };
                        user.Items.Add(newItem);

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
                    // Fully limit broken (or R): pay out body labels instead of a spare body.
                    gacha.CurrencyValue = characterData.OriginalRare == OriginalRareType.SSR ? 6000 :
                                          characterData.OriginalRare == OriginalRareType.SR ? 200 : 150;
                    totalBodyLabels += (int)gacha.CurrencyValue;
                }
            }
            else
            {
                // New character: hand out the character itself, no spare body and no body labels.
                long newCsn = user.GenerateUniqueCharacterId();
                gacha.Sn = newCsn;

                response.Characters.Add(new NetUserCharacterDefaultData()
                {
                    CostumeId = 0,
                    Csn = newCsn,
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
                    Csn = (int)newCsn,
                    Grade = 0,
                    Level = 1,
                    Skill1Lvl = 1,
                    Skill2Lvl = 1,
                    Tid = characterData.Id,
                    UltimateLevel = 1
                });

                user.AddBadge(BadgeContents.NikkeNew, characterData.NameCode.ToString());
                user.AddTrigger(Trigger.ObtainCharacter, 1, characterData.NameCode);
                user.AddTrigger(Trigger.ObtainCharacterNew, 1);
            }

            gacha.Corporation = 1;
            response.Gacha.Add(gacha);

            user.AddTrigger(Trigger.GachaCharacter, 0, 0);
        }

        if (totalBodyLabels > 0)
        {
            user.AddCurrency(CurrencyType.DissolutionPoint, totalBodyLabels);
            response.Currencies.Add(new NetUserCurrencyData
            {
                Type = (int)CurrencyType.DissolutionPoint,
                Value = user.GetCurrencyVal(CurrencyType.DissolutionPoint)
            });
        }

        user.GachaDailyFreePulls.Add(req.EventId);

        user.AddGachaPullCount(bannerID, numberOfPulls);

        JsonDb.Save();

        await WriteDataAsync(response);
    }

}
