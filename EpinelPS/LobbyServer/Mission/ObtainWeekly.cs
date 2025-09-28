using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/obtain/weekly")]
    public class ObtainWeekly : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainWeeklyMissionReward req = await ReadData<ReqObtainWeeklyMissionReward>();
            User user = GetUser();

            ResObtainWeeklyMissionReward response = new();

            List<NetRewardData> rewards = [];

            int total_points = 0;

            foreach (int item in req.TidList)
            {
                if (user.WeeklyResetableData.CompletedWeeklyMissions.Contains(item)) continue;

                if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key)) throw new Exception("unknown TID");

                user.WeeklyResetableData.CompletedWeeklyMissions.Add(item);

                if (key.reward_id != 0)
                {
                    // Actual reward
                    RewardRecord rewardRecord = GameData.Instance.GetRewardTableEntry(key.reward_id) ?? throw new Exception("unable to lookup reward");
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, rewardRecord));
                }
                else
                {
                    // Point reward
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
