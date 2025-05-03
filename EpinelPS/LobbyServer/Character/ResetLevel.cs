using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/growreset")]
    public class ResetLevel : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCharacterGrowReset>();
            var user = GetUser();
            var response = new ResCharacterGrowReset();
            var data = GameData.Instance.GetCharacterLevelUpData();

            foreach (var item in user.Characters.ToArray())
            {
                if (item.Csn == req.Csn)
                {
                    if (item.Level == 1)
                    {
                        Console.WriteLine("Character level is already 1 - cannot reset");
                        return;
                    }
                    if (item.Level == 200)
                    {
                        Console.WriteLine("Character level is 200 - cannot reset");
                        return;
                    }

                    int requiredCredit = 0;
                    int requiredBattleData = 0;
                    int requiredCoreDust = 0;
                    for (int i = 1; i < item.Level; i++)
                    {
                        var levelUpData = data[i];
                        requiredCredit += levelUpData.gold;
                        requiredBattleData += levelUpData.character_exp;
                        requiredCoreDust += levelUpData.character_exp2;
                    }

                    user.AddCurrency(CurrencyType.Gold, requiredCredit);
                    user.AddCurrency(CurrencyType.CharacterExp, requiredBattleData);
                    user.AddCurrency(CurrencyType.CharacterExp2, requiredCoreDust);
                    item.Level = 1;

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

                    response.SynchroLv = highestLevelCharacters.Last().Level;

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
