using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/obtain/achievement")]
    public class ObtainAchievement : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainAchievementReward>();
            var user = GetUser();

            var response = new ResObtainAchievementReward();

            List<NetRewardData> rewards = [];

            int total_points = 0;

            foreach (var item in req.TidList)
            {
                if (user.CompletedAchievements.Contains(item)) continue;

                if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key)) throw new Exception("unknown TID");

                var rewardRecord = GameData.Instance.GetRewardTableEntry(key.reward_id) ?? throw new Exception("unable to lookup reward");

                var reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
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
