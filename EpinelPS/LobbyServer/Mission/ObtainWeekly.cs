using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/obtain/weekly")]
    public class ObtainWeekly : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainWeeklyMissionReward>();
            var user = GetUser();

            var response = new ResObtainWeeklyMissionReward();

            List<NetRewardData> rewards = new();

            int total_points = 0;

            foreach (var item in req.TidList)
            {
                if (user.WeeklyResetableData.CompletedWeeklyMissions.Contains(item)) continue;

                if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key)) throw new Exception("unknown TID");

                user.WeeklyResetableData.CompletedWeeklyMissions.Add(item);

                if (key.reward_id != 0)
                {
                    // Actual reward
                    var rewardRecord = GameData.Instance.GetRewardTableEntry(key.reward_id) ?? throw new Exception("unable to lookup reward");
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, rewardRecord));
                }
                else
                {
                    // Point reward

                    var reward = new NetRewardData();

                    rewards.Add(reward);

                    total_points += key.point_value;
                }
            }

            user.AddTrigger(TriggerType.PointRewardWeekly, total_points);
            user.WeeklyResetableData.WeeklyMissionPoints += total_points;

            response.Reward = NetUtils.MergeRewards(rewards, user);
            response.EventBonusReward = new() { PassPoint = new() };
            response.Reward.PassPoint = new();

            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
