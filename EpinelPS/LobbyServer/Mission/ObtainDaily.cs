using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission;

[GameRequest("/mission/obtain/daily")]
public class ObtainDaily : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainDailyMissionReward req = await ReadData<ReqObtainDailyMissionReward>();
        User user = GetUser();

        ResObtainDailyMissionReward response = new();

        List<NetRewardData> rewards = [];

        int total_points = 0;

        foreach (int item in req.TidList)
        {
            if (user.ResetableData.CompletedDailyMissions.Contains(item))
            {
                Logging.WriteLine("already completed daily mission", LogType.Warning);
                continue;
            }

            if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key))
            {
                Logging.Warn($"[ObtainDaily] Unknown daily mission TID: {item}");
                continue;
            }

            user.ResetableData.CompletedDailyMissions.Add(item);

            if (key.RewardId != 0)
            {
                // Milestone chest with direct rewards
                RewardRecord? rewardRecord = GameData.Instance.GetRewardTableEntry(key.RewardId);
                if (rewardRecord != null)
                {
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, rewardRecord));
                }
            }
            else
            {
                // Daily task granting mission points
                total_points += key.PointValue;
            }
        }

        if (total_points > 0)
        {
            user.AddTrigger(Trigger.PointRewardDaily, total_points);
            user.ResetableData.DailyMissionPoints += total_points;
        }

        response.Reward = NetUtils.MergeRewards(rewards, user);
        response.EventBonusReward = new() { PassPoint = new() };
        response.Reward.PassPoint = new();

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
