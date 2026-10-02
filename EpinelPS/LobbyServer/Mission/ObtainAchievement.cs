using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission;

[GameRequest("/mission/obtain/achievement")]
public class ObtainAchievement : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainAchievementReward req = await ReadData<ReqObtainAchievementReward>();
        User user = GetUser();

        ResObtainAchievementReward response = new();

        List<NetRewardData> rewards = [];

        int total_points = 0;

        foreach (int item in req.TidList)
        {
            if (user.CompletedAchievements.Contains(item)) continue;

            if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key))
            {
                Logging.Warn($"[ObtainAchievement] Unknown achievement TID: {item}");
                continue;
            }

            RewardRecord? rewardRecord = GameData.Instance.GetRewardTableEntry(key.RewardId);
            if (rewardRecord != null)
            {
                NetRewardData reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
                rewards.Add(reward);
            }
            else
            {
                Logging.Warn($"[ObtainAchievement] Unable to find reward for TID {item} with RewardId {key.RewardId}");
            }

            user.CompletedAchievements.Add(item);

            // Accumulate achievement progress points toward milestone chests ("Complete XX Challenge(s)"):
            // Each regular challenge completed awards exactly 1 point.
            // Milestone chests themselves (PointRewardAchievement) do not recursively award milestone points.
            if (key.Trigger != Trigger.PointRewardAchievement)
            {
                total_points += 1;
            }
        }

        if (total_points > 0)
        {
            user.AddTrigger(Trigger.PointRewardAchievement, total_points);
        }

        response.Reward = NetUtils.MergeRewards(rewards, user);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
