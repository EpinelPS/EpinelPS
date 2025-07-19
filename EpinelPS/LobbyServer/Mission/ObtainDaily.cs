using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/obtain/daily")]
    public class ObtainDaily : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainDailyMissionReward>();
            var user = GetUser();

            var response = new ResObtainDailyMissionReward();

            List<NetRewardData> rewards = [];

            int total_points = 0;

            foreach (var item in req.TidList)
            {
                if (user.ResetableData.CompletedDailyMissions.Contains(item))
                {
                    Logging.WriteLine("already completed daily mission", LogType.Warning);
                    continue;
                }

                if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key)) throw new Exception("unknown TID");

                user.ResetableData.CompletedDailyMissions.Add(item);

                if (key.reward_id != 0)
                {
                    // Actual reward
                    var rewardRecord = GameData.Instance.GetRewardTableEntry(key.reward_id) ?? throw new Exception("unable to lookup reward");
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, rewardRecord));
                }
                else
                {
                    // Point reward
                    total_points += key.point_value;
                }
            }

            user.AddTrigger(TriggerType.PointRewardDaily, total_points);
            user.ResetableData.DailyMissionPoints += total_points;

            response.Reward = NetUtils.MergeRewards(rewards, user);
            response.EventBonusReward = new() { PassPoint = new() };
            response.Reward.PassPoint = new();

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
