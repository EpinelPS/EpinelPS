using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission;

[GameRequest("/mission/obtain/weekly")]
public class ObtainWeekly : LobbyMessage
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

            if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key))
            {
                Logging.Warn($"[ObtainWeekly] Unknown weekly mission TID: {item}");
                continue;
            }

            user.WeeklyResetableData.CompletedWeeklyMissions.Add(item);

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
                // Weekly task granting mission points
                total_points += key.PointValue;
            }
        }

        if (total_points > 0)
        {
            user.AddTrigger(Trigger.PointRewardWeekly, total_points);
            user.WeeklyResetableData.WeeklyMissionPoints += total_points;
        }

        response.Reward = NetUtils.MergeRewards(rewards, user);
        response.EventBonusReward = new() { PassPoint = new() };
        response.Reward.PassPoint = new();

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
