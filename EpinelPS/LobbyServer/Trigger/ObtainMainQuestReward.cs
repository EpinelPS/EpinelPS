using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Data;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.TriggerController
{
    [PacketPath("/trigger/obtainmainquestreward")]
    public class ObtainMainQuestReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainMainQuestReward req = await ReadData<ReqObtainMainQuestReward>();
            User user = GetUser();

            ResObtainMainQuestReward response = new();
            List<NetRewardData> rewards = [];

            foreach (KeyValuePair<int, bool> item in user.MainQuestData)
            {
                // give only rewards for things that were completed and not claimed already
                if (!item.Value && req.TidList.Contains(item.Key))
                {
                    user.MainQuestData[item.Key] = true;

                    MainQuestRecord? questInfo = GameData.Instance.GetMainQuestByTableId(item.Key) ?? throw new Exception("failed to lookup quest Id " + item.Key);
                    RewardRecord? reward = GameData.Instance.GetRewardTableEntry(questInfo.RewardId) ?? throw new Exception("failed to lookup reward Id " + questInfo.RewardId);
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, reward));
                }
            }

            response.Reward = NetUtils.MergeRewards(rewards, user);

            foreach (NetItemData? item in response.Reward.Item)
            {
                Console.WriteLine($"item: {item.Tid} {item.Isn} {item.Count}");
            }
            
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
