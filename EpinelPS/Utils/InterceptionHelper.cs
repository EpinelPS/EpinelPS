using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.Utils
{
    public static class InterceptionHelper
    {
        public static InterceptionClearResult Clear(User user, int type, int id, long damage = 0)
        {
             InterceptionClearResult response = new();

            if (type != 0 && type != 1) throw new Exception("unknown type");

            Dictionary<int, InterceptionRecord> records = type == 0 ? GameData.Instance.InterceptNormal : GameData.Instance.InterceptSpecial;

            var record = records[id];

            var normReward = GameData.Instance.GetConditionReward(record.condition_reward_group, damage);
            if (normReward != 0)
            {
                response.NormalReward = RewardUtils.RegisterRewardsForUser(user, normReward);
            }
            else
            {
                Logging.WriteLine($"unable to find reward which meets condition of damage {damage} and group {record.condition_reward_group}");
            }

            var percentReward = GameData.Instance.GetConditionReward(record.percent_condition_reward_group, damage);
            if (percentReward != 0)
            {
                var r = RewardUtils.RegisterRewardsForUser(user, normReward);
                response.NormalReward = NetUtils.MergeRewards([response.NormalReward, r], user);
            }
            else
            {
                Logging.WriteLine($"unable to find reward which meets condition of damage {damage} and group {record.condition_reward_group}");
            }

            JsonDb.Save();

            return response;
        }
    }

    public class InterceptionClearResult
    {
        public NetRewardData NormalReward = new();
    }
}
