using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.Utils
{
    public static class InterceptionHelper
    {
        public static InterceptionClearResult Clear(User user, int type, int id, long damage = 0)
        {
             InterceptionClearResult response = new();

            //if (type != 1 && type != 2) throw new Exception("unknown interception type");

            int conditionReward;
            int percentRewardGroup;
            if (type == 0 || type == 1)
            {
                conditionReward = GameData.Instance.InterceptNormal[id].ConditionRewardGroup;
                percentRewardGroup = GameData.Instance.InterceptNormal[id].PercentConditionRewardGroup;
            }
            else
            {
                conditionReward = GameData.Instance.InterceptSpecial[id].ConditionRewardGroup;
                percentRewardGroup = GameData.Instance.InterceptSpecial[id].PercentConditionRewardGroup;
            }


            int normReward = GameData.Instance.GetConditionReward(conditionReward, damage);
            if (normReward != 0)
            {
                response.NormalReward = RewardUtils.RegisterRewardsForUser(user, normReward);
            }
            else
            {
                Logging.WriteLine($"unable to find reward which meets condition of damage {damage} and group {conditionReward}");
            }

            int percentReward = GameData.Instance.GetConditionReward(percentRewardGroup, damage);
            if (percentReward != 0)
            {
                response.BonusReward = RewardUtils.RegisterRewardsForUser(user, percentReward);
            }

            JsonDb.Save();

            return response;
        }
    }

    public class InterceptionClearResult
    {
        public NetRewardData NormalReward = new();
        public NetRewardData BonusReward = new();
    }
}
