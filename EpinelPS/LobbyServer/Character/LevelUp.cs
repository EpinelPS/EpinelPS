using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/levelup")]
    public class LevelUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCharacterLevelUp req = await ReadData<ReqCharacterLevelUp>();
            User user = GetUser();
            ResCharacterLevelUp response = new();
            Dictionary<int, CharacterLevelData> data = GameData.Instance.GetCharacterLevelUpData();

            foreach (CharacterModel item in user.Characters.ToArray())
            {
                if (item.Csn == req.Csn)
                {
                    int requiredCredit = 0;
                    int requiredBattleData = 0;
                    int requiredCoreDust = 0;
                    for (int i = item.Level; i < req.Lv; i++)
                    {
                        CharacterLevelData levelUpData = data[i];
                        requiredCredit += levelUpData.gold;
                        requiredBattleData += levelUpData.character_exp;
                        requiredCoreDust += levelUpData.character_exp2;
                    }

                    if (user.CanSubtractCurrency(CurrencyType.Gold, requiredCredit) &&
                        user.CanSubtractCurrency(CurrencyType.CharacterExp, requiredBattleData) &&
                        user.CanSubtractCurrency(CurrencyType.CharacterExp2, requiredCoreDust))
                    {
                        user.SubtractCurrency(CurrencyType.Gold, requiredCredit);
                        user.SubtractCurrency(CurrencyType.CharacterExp, requiredBattleData);
                        user.SubtractCurrency(CurrencyType.CharacterExp2, requiredCoreDust);
                        item.Level = req.Lv;
                    }
                    else
                    {
                        Logging.WriteLine("ERROR: Not enough currency for upgrade", LogType.WarningAntiCheat);
                        return;
                    }

                    response.Character = new()
                    {
                        CostumeId = item.CostumeId,
                        Csn = item.Csn,
                        Lv = item.Level,
                        Skill1Lv = item.Skill1Lvl,
                        Skill2Lv = item.Skill2Lvl,
                        UltiSkillLv = item.UltimateLevel,
                        Grade = item.Grade,
                        Tid = item.Tid
                    };
                    List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];

                    response.SynchroLv = user.GetSynchroLevel();

                    foreach (CharacterModel? c in highestLevelCharacters)
                    {
                        response.SynchroStandardCharacters.Add(c.Csn);
                    }

                    foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
                    {
                        response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
                    }

                    break;
                }
            }

            user.AddTrigger(TriggerType.CharacterLevelUpCount, 1);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
