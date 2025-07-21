using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/SynchroDevice/LevelUp")]
    public class SynchroLevelUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSynchroLevelUp req = await ReadData<ReqSynchroLevelUp>();
            User user = GetUser();

            ResSynchroLevelUp response = new();
            Dictionary<int, CharacterLevelData> data = GameData.Instance.GetCharacterLevelUpData();


            int requiredCredit = 0;
            int requiredBattleData = 0;
            int requiredCoreDust = 0;
            CharacterLevelData levelUpData = data[user.SynchroDeviceLevel + 1];
            requiredCredit += levelUpData.gold;
            requiredBattleData += levelUpData.character_exp;
            requiredCoreDust += levelUpData.character_exp2;

            if (user.CanSubtractCurrency(CurrencyType.Gold, requiredCredit) &&
                user.CanSubtractCurrency(CurrencyType.CharacterExp, requiredBattleData) &&
                user.CanSubtractCurrency(CurrencyType.CharacterExp2, requiredCoreDust))
            {
                user.SubtractCurrency(CurrencyType.Gold, requiredCredit);
                user.SubtractCurrency(CurrencyType.CharacterExp, requiredBattleData);
                user.SubtractCurrency(CurrencyType.CharacterExp2, requiredCoreDust);
                user.SynchroDeviceLevel++;
            }
            else
            {
                // TOOD: log this
                Console.WriteLine("ERROR: Not enough currency for upgrade");
                return;
            }


            foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
            {
                response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
            }
            response.SynchroLv = user.SynchroDeviceLevel;

            user.AddTrigger(TriggerType.CharacterLevelUpCount, 1);

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
