using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/obtain/achievement")]
    public class ObtainAchievement : LobbyMsgHandler
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

                if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key)) throw new Exception("unknown TID");

                RewardTableRecord rewardRecord = GameData.Instance.GetRewardTableEntry(key.reward_id) ?? throw new Exception("unable to lookup reward");

                NetRewardData reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
                rewards.Add(reward);

                user.CompletedAchievements.Add(item);

                total_points++;
            }

            user.AddTrigger(TriggerType.PointRewardAchievement, total_points);

            response.Reward = NetUtils.MergeRewards(rewards, user);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
