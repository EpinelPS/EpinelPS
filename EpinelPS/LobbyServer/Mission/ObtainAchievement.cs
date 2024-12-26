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

            List<NetRewardData> rewards = new();

            foreach (var item in req.TidList)
            {
                if (user.CompletedAchievements.Contains(item)) continue;

                if (!GameData.Instance.TriggerTable.TryGetValue(item, out TriggerRecord? key)) throw new Exception("unknown TID");

                var rewardRecord = GameData.Instance.GetRewardTableEntry(key.reward_id) ?? throw new Exception("unable to lookup reward");

                rewards.Add(RewardUtils.RegisterRewardsForUser(user, rewardRecord));

                user.CompletedAchievements.Add(item);
            }

            response.Reward = NetUtils.MergeRewards(rewards, user);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
