using nksrv.Database;
using nksrv.LobbyServer.Msgs.Stage;
using nksrv.StaticInfo;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Trigger
{
    [PacketPath("/trigger/obtainmainquestreward")]
    public class ObtainMainQuestReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainMainQuestReward>();
            var user = GetUser();

            ResObtainMainQuestReward response = new();
            List<NetRewardData> rewards = new();

            foreach (var item in user.MainQuestData)
            {
                if (!item.Value && req.TidList.Contains(item.Key))
                {
                    user.MainQuestData[item.Key] = true;

                    var questInfo = StaticDataParser.Instance.GetMainQuestByTableId(item.Key);
                    if (questInfo == null) throw new Exception("failed to lookup quest id " + item.Key);
                    var reward = StaticDataParser.Instance.GetRewardTableEntry(questInfo.reward_id);
                    if (reward == null) throw new Exception("failed to lookup reward id " + questInfo.reward_id);

                    rewards.Add(ClearStage.RegisterRewardsForUser(user, reward));
                }
            }

            response.Reward = NetUtils.MergeRewards(rewards, user);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
