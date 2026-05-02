using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/obtainfastbattlereward")]
public class DoWipeout : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainFastBattleReward req = await ReadData<ReqObtainFastBattleReward>();
        ResObtainFastBattleReward response = new();
        User user = GetUser();

        if (user.ResetableData.WipeoutCount >= 12)
        {
            throw new InvalidOperationException("wipeout count cannot exceed 12.");
        }

        user.ResetableData.WipeoutCount++;
        response.FastBattleCount = user.ResetableData.WipeoutCount;

        response.Reward = NetUtils.GetOutpostReward(user, TimeSpan.FromHours(2));
        NetUtils.RegisterRewardsForUser(user, response.Reward);

        // TODO subtract currency as needed
        foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
        {
            response.Currencies.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
        }

        user.AddTrigger(Trigger.OutpostFastBattleReward, 1);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
