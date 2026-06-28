using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp;

internal static class InAppPurchaseStateService
{
    public static void ApplySuccessfulPurchase(User user, string productId, string source)
    {
        MidasProductRecord? midas = PackagePurchaseResolver.ResolveMidasProduct(productId);
        if (midas == null)
        {
            return;
        }

        switch (midas.ProductType)
        {
            case ProductType.MonthlyAmount:
                ActivateMonthlyAmount(user, midas, source);
                break;

            case ProductType.CampaignPackageShop:
                MarkCampaignPurchaseRewards(user, midas.ProductId, source);
                break;
        }
    }

    public static bool AppendMonthlySubscriptionData(User user, ResGetMonthlySubscriptionReward response, string source)
    {
        bool changed = false;
        long nowTicks = DateTime.UtcNow.Ticks;
        int today = user.GetDateDay();

        foreach (MonthlySubscriptionState state in user.MonthlySubscriptions.Values
            .Where(item => item.ExpiredAt > nowTicks)
            .OrderBy(item => item.Tid)
            .ToList())
        {
            response.DataList.Add(new NetMonthlySubscriptionData()
            {
                Tid = state.Tid,
                ExpiredAt = state.ExpiredAt
            });

            if (state.LastRewardDateDay == today)
            {
                continue;
            }

            if (!GameData.Instance.MonthlyAmountTable.TryGetValue(state.Tid, out MonthlyAmountRecord? monthly))
            {
                Logging.WriteLine($"{source} missing monthly amount tid={state.Tid}", LogType.Warning);
                continue;
            }

            NetRewardData reward = PackageRewardBuilder.BuildPackageGroupReward(
                user,
                monthly.DailyPackageGroupId,
                $"{source}/daily");

            response.RewardList.Add(new NetMonthlySubscriptionReward()
            {
                Tid = monthly.Id,
                Reward = reward
            });

            state.LastRewardDateDay = today;
            changed = true;
        }

        return changed;
    }

    public static IReadOnlyList<NetCampaignPackageReward> BuildCampaignAlreadyRewards(User user)
    {
        return user.CampaignPackageRewards
            .OrderBy(item => item.PackageShopId)
            .ThenBy(item => item.PackageGroupTableId)
            .Select(item => new NetCampaignPackageReward()
            {
                PackageShopId = item.PackageShopId,
                PackageGroupTableId = item.PackageGroupTableId
            })
            .ToList();
    }

    public static void MarkCampaignRewardRows(User user, int packageGroupId, IEnumerable<int> packageGroupTableIds, string source)
    {
        int packageShopId = ResolveCampaignPackageShopId(packageGroupId);

        foreach (int packageGroupTableId in packageGroupTableIds)
        {
            if (!GameData.Instance.CampaignPackageGroupTable.TryGetValue(packageGroupTableId, out CampaignPackageGroupRecord? reward) ||
                reward.PackageGroupId != packageGroupId)
            {
                continue;
            }

            if (user.CampaignPackageRewards.Any(item =>
                item.PackageShopId == packageShopId &&
                item.PackageGroupTableId == packageGroupTableId))
            {
                continue;
            }

            user.CampaignPackageRewards.Add(new CampaignPackageRewardState()
            {
                PackageShopId = packageShopId,
                PackageGroupTableId = packageGroupTableId
            });
        }
    }

    private static void ActivateMonthlyAmount(User user, MidasProductRecord midas, string source)
    {
        MonthlyAmountRecord? monthly = ResolveMonthlyAmount(midas);
        if (monthly == null)
        {
            Logging.WriteLine(
                $"{source} monthly amount state missing productId={midas.MidasProductIdProximabeta} midasId={midas.Id} monthlyProductId={midas.ProductId}",
                LogType.Warning);
            return;
        }

        int period = Math.Max(1, monthly.Period);
        long nowTicks = DateTime.UtcNow.Ticks;
        if (!user.MonthlySubscriptions.TryGetValue(monthly.Id, out MonthlySubscriptionState? state))
        {
            state = new MonthlySubscriptionState()
            {
                Tid = monthly.Id,
                LastRewardDateDay = 0
            };
            user.MonthlySubscriptions[monthly.Id] = state;
        }

        long startTicks = Math.Max(nowTicks, state.ExpiredAt);
        state.ExpiredAt = new DateTime(startTicks, DateTimeKind.Utc).AddDays(period).Ticks;
    }

    private static void MarkCampaignPurchaseRewards(User user, int packageGroupId, string source)
    {
        List<int> purchaseRewardIds = GameData.Instance.CampaignPackageGroupTable.Values
            .Where(item => item.PackageGroupId == packageGroupId && item.RewardType == CampaignPackageRewardType.Purchase)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToList();

        MarkCampaignRewardRows(user, packageGroupId, purchaseRewardIds, source);
    }

    private static MonthlyAmountRecord? ResolveMonthlyAmount(MidasProductRecord midas)
    {
        return GameData.Instance.MonthlyAmountTable.Values.FirstOrDefault(item =>
            item.Id == midas.ProductId ||
            item.MidasProductId == midas.Id);
    }

    private static int ResolveCampaignPackageShopId(int packageGroupId)
    {
        return GameData.Instance.CampaignPackageShopTable.Values
            .Where(item => item.PackageGroupId == packageGroupId)
            .OrderBy(item => item.PackageOrder)
            .ThenBy(item => item.Id)
            .FirstOrDefault()
            ?.Id ?? packageGroupId;
    }
}
