using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.Utils;

public static class InterceptionHelper
{
    public static InterceptionClearResult Clear(User user, int type, int id, long damage = 0)
    {
        if (type == (int)InterceptType.InterceptAnomalous || type == 3)
        {
            return ClearAnomalous(user, id, damage);
        }

        InterceptionClearResult response = new();

        int conditionReward = 0;
        int percentRewardGroup = 0;

        if (type == (int)InterceptType.InterceptNormalLevel1 || type == (int)InterceptType.InterceptNormalLevel2 || type == 0 || type == 1)
        {
            if (GameData.Instance.InterceptNormal.TryGetValue(id, out var normRecord))
            {
                conditionReward = normRecord.ConditionRewardGroup;
                percentRewardGroup = normRecord.PercentConditionRewardGroup;
            }
            else
            {
                var fallback = GameData.Instance.InterceptNormal.Values.FirstOrDefault(x => x.Order == id)
                    ?? GameData.Instance.InterceptNormal.Values.FirstOrDefault();
                if (fallback != null)
                {
                    conditionReward = fallback.ConditionRewardGroup;
                    percentRewardGroup = fallback.PercentConditionRewardGroup;
                }
            }
        }
        else
        {
            if (id == 0)
            {
                id = LobbyServer.Intercept.GetInterceptData.GetCurrentSpecialInterceptionId();
            }

            if (GameData.Instance.InterceptSpecial.TryGetValue(id, out var specRecord))
            {
                conditionReward = specRecord.ConditionRewardGroup;
                percentRewardGroup = specRecord.PercentConditionRewardGroup;
            }
            else
            {
                var fallback = GameData.Instance.InterceptSpecial.Values.FirstOrDefault(x => x.Order == id)
                    ?? GameData.Instance.InterceptSpecial.Values.FirstOrDefault();
                if (fallback != null)
                {
                    conditionReward = fallback.ConditionRewardGroup;
                    percentRewardGroup = fallback.PercentConditionRewardGroup;
                }
            }
        }

        int normReward = GameData.Instance.GetConditionReward(conditionReward, damage);
        if (normReward != 0)
        {
            response.NormalReward = RewardUtils.RegisterRewardsForUser(user, normReward);
        }
        else
        {
            Logging.WriteLine($"Unable to find reward which meets condition of damage {damage} and group {conditionReward}", LogType.Warning);
        }

        int percentReward = GameData.Instance.GetConditionReward(percentRewardGroup, damage);
        if (percentReward != 0)
        {
            response.BonusReward = RewardUtils.RegisterRewardsForUser(user, percentReward, rollPercentages: true);
        }

        JsonDb.Save();

        return response;
    }

    public static InterceptionClearResult ClearAnomalous(User user, int id, long damage = 0)
    {
        InterceptionClearResult response = new();

        if (!GameData.Instance.InterceptAnomalous.TryGetValue(id, out var record))
        {
            record = GameData.Instance.InterceptAnomalous.Values.FirstOrDefault(x => x.Id == id || x.Order == id);
            if (record == null)
            {
                Logging.WriteLine($"Unknown Anomalous Intercept Id: {id}", LogType.Error);
                return response;
            }
        }

        // Normal reward based on achieved damage/stage
        int conditionReward = record.ConditionRewardGroup;
        int normReward = GameData.Instance.GetConditionReward(conditionReward, damage);
        if (normReward != 0)
        {
            response.NormalReward = RewardUtils.RegisterRewardsForUser(user, normReward);
        }
        else
        {
            Logging.WriteLine($"Unable to find anomalous reward which meets condition of damage {damage} and group {conditionReward}", LogType.Warning);
        }

        // Bonus rewards (gear + custom module / blue crystal drop pools)
        if (record.PercentConditionRewardGroups != null && record.PercentConditionRewardGroups.Count > 0)
        {
            List<NetRewardData> bonusList = [];
            foreach (var pctGroup in record.PercentConditionRewardGroups)
            {
                int pctReward = GameData.Instance.GetConditionReward(pctGroup.PercentConditionRewardGroup, damage);
                if (pctReward != 0)
                {
                    NetRewardData bonus = RewardUtils.RegisterRewardsForUser(user, pctReward, rollPercentages: true);
                    bonusList.Add(bonus);
                }
            }

            if (bonusList.Count > 0)
            {
                response.BonusReward = NetUtils.MergeRewards(bonusList, user);
            }
        }

        JsonDb.Save();
        return response;
    }

    public static long GetMaxDamageForNormalOrSpecial(int type, int id)
    {
        int conditionRewardGroup = 0;
        if (type == (int)InterceptType.InterceptNormalLevel1 || type == (int)InterceptType.InterceptNormalLevel2 || type == 0 || type == 1)
        {
            if (GameData.Instance.InterceptNormal.TryGetValue(id, out var norm))
                conditionRewardGroup = norm.ConditionRewardGroup;
            else if (GameData.Instance.InterceptNormal.Count > 0)
                conditionRewardGroup = GameData.Instance.InterceptNormal.Values.First().ConditionRewardGroup;
        }
        else
        {
            if (id == 0)
            {
                id = LobbyServer.Intercept.GetInterceptData.GetCurrentSpecialInterceptionId();
            }

            if (GameData.Instance.InterceptSpecial.TryGetValue(id, out var spec))
                conditionRewardGroup = spec.ConditionRewardGroup;
            else if (GameData.Instance.InterceptSpecial.Count > 0)
                conditionRewardGroup = GameData.Instance.InterceptSpecial.Values.First().ConditionRewardGroup;
        }

        return GetMaxDamageForGroup(conditionRewardGroup);
    }

    public static long GetMaxDamageForAnomalous(int id)
    {
        if (GameData.Instance.InterceptAnomalous.TryGetValue(id, out var record) ||
            (record = GameData.Instance.InterceptAnomalous.Values.FirstOrDefault(x => x.Id == id || x.Order == id)) != null)
        {
            return GetMaxDamageForGroup(record.ConditionRewardGroup);
        }

        return 5800000001; // Stage 9 damage fallback
    }

    public static long GetMaxDamageForGroup(int conditionRewardGroup)
    {
        var rewards = GameData.Instance.ConditionRewards.Values
            .Where(x => x.Group == conditionRewardGroup)
            .OrderByDescending(x => x.Priority)
            .ToList();

        if (rewards.Count > 0)
        {
            return rewards.First().ValueMin;
        }

        return 0;
    }
}

public class InterceptionClearResult
{
    public NetRewardData NormalReward = new();
    public NetRewardData BonusReward = new();
}
