using EpinelPS.Data;

namespace EpinelPS.Interfaces;

public interface IInventoryService
{
    long GetCurrencyAmount(GameUser gameUser, CurrencyType type);
    long AddCurrency(GameUser gameUser, CurrencyType type, long amount);
    NetRewardData AddReward(GameUser gameUser, int rewardId);
    NetRewardData AddReward(GameUser gameUser, RewardRecord rewardData);
    NetRewardData MergeRewards(GameUser gameUser,  List<NetRewardData> rewards);
}