﻿using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/obtainfastbattlereward")]
    public class DoWipeout : LobbyMsgHandler
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

            user.AddTrigger(TriggerType.OutpostFastBattleReward, 1);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
