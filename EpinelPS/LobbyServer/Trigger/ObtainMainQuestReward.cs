using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Trigger
{
    [PacketPath("/trigger/obtainmainquestreward")]
    public class ObtainMainQuestReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainMainQuestReward>();
            var user = GetUser();

            ResObtainMainQuestReward response = new();
            List<NetRewardData> rewards = [];

            foreach (var item in user.MainQuestData)
            {
                // give only rewards for things that were completed and not claimed already
                if (!item.Value && req.TidList.Contains(item.Key))
                {
                    user.MainQuestData[item.Key] = true;

                    var questInfo = GameData.Instance.GetMainQuestByTableId(item.Key);
                    if (questInfo == null) throw new Exception("failed to lookup quest id " + item.Key);
                    var reward = GameData.Instance.GetRewardTableEntry(questInfo.reward_id);
                    if (reward == null) throw new Exception("failed to lookup reward id " + questInfo.reward_id);

                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, reward));
                }
            }

            response.Reward = NetUtils.MergeRewards(rewards, user);

            foreach (var item in response.Reward.Item)
            {
                Console.WriteLine($"item: {item.Tid} {item.Isn} {item.Count}");
            }
            
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
