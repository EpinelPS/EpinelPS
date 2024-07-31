using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;
using Swan.Logging;

namespace EpinelPS.LobbyServer.Msgs.Character
{
    [PacketPath("/character/levelup")]
    public class LevelUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCharacterLevelUp>();
            var user = GetUser();
            var response = new ResCharacterLevelUp();
            var data = GameData.Instance.GetCharacterLevelUpData();

            foreach (var item in user.Characters.ToArray())
            {
                if (item.Csn == req.Csn)
                {
                    int requiredCredit = 0;
                    int requiredBattleData = 0;
                    int requiredCoreDust = 0;
                    for (int i = item.Level; i < req.Level; i++)
                    {
                        var levelUpData = data[i];
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
                        item.Level = req.Level;
                    }
                    else
                    {
                        // TOOD: log this
                        Logger.Error("ERROR: Not enough currency for upgrade");
                        return;
                    }

                    response.Character = new()
                    {
                        CostumeId = item.CostumeId,
                        Csn = item.Csn,
                        Level = item.Level,
                        Skill1Lv = item.Skill1Lvl,
                        Skill2Lv = item.Skill2Lvl,
                        UltiSkillLv = item.UltimateLevel,
                        Grade = item.Grade,
                        Tid = item.Tid
                    };
                    var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();

                    response.SynchroLv = user.GetSynchroLevel();

                    foreach (var c in highestLevelCharacters)
                    {
                        response.SynchroStandardCharacters.Add(c.Csn);
                    }

                    foreach (var currency in user.Currency)
                    {
                        response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
                    }

                    break;
                }
            }
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
