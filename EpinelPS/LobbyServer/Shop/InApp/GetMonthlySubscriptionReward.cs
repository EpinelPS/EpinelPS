using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getmonthlysubscriptionreward")]
public class GetMonthlySubscriptionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetMonthlySubscriptionReward>();

        User user = GetUser();
        user.ResetDataIfNeeded();

        ResGetMonthlySubscriptionReward response = new();

        DateTime now = DateTime.UtcNow;
        bool changed = false;

        foreach (var (tid, expiry) in user.MonthlySubscriptions.ToList())
        {
            // Backward compatibility: ensure remaining claims counter is initialized
            if (!user.MonthlySubscriptionRemainingClaims.TryGetValue(tid, out var remainingClaims))
            {
                if (expiry > now && GameData.Instance.MonthlyAmountTable.TryGetValue(tid, out var mRecord))
                {
                    remainingClaims = mRecord.Period > 0 ? mRecord.Period : 30;
                    user.MonthlySubscriptionRemainingClaims[tid] = remainingClaims;
                }
                else
                {
                    remainingClaims = 0;
                }
            }

            if (remainingClaims <= 0)
                continue;

            response.DataList.Add(new NetMonthlySubscriptionData
            {
                Tid = tid,
                ExpiredAt = expiry.Ticks,
            });

            // Once per server session (or refreshed by daily reset)
            if (MonthlySubscriptionSessionTracker.CanClaim(user, tid) &&
                GameData.Instance.MonthlyAmountTable.TryGetValue(tid, out var monthly))
            {
                NetRewardData dailyReward = new() { PassPoint = new NetPassPointData() };
                // Dynamically grants daily items from PackageGroupTable based on monthly.DailyPackageGroupId
                if (InAppPurchaseHelper.GrantPackageGroup(user, monthly.DailyPackageGroupId, ref dailyReward, allowEmpty: true))
                {
                    response.RewardList.Add(new NetMonthlySubscriptionReward
                    {
                        Tid = tid,
                        Reward = dailyReward,
                    });

                    MonthlySubscriptionSessionTracker.MarkClaimed(user, tid);
                    user.MonthlySubscriptionRemainingClaims[tid] = remainingClaims - 1;
                    user.MonthlySubscriptionLastClaimed[tid] = now;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
