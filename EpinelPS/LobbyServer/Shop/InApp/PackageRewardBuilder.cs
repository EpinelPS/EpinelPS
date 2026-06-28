using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp;

internal static class PackageRewardBuilder
{
    public static NetRewardData BuildPurchaseReward(User user, string productId, int packageShopId, int packageListId, string source)
    {
        MidasProductRecord? midas = PackagePurchaseResolver.ResolveMidasProduct(productId);
        if (midas != null)
        {
            if (midas.ProductType == ProductType.CashShop &&
                TryBuildCashShopReward(user, midas, source, out NetRewardData cashShopReward))
            {
                return cashShopReward;
            }

            if (midas.ProductType == ProductType.CampaignPackageShop &&
                TryBuildCampaignPurchaseReward(user, midas.ProductId, source, out NetRewardData campaignReward))
            {
                return campaignReward;
            }

            if (midas.ProductType == ProductType.MonthlyAmount &&
                TryBuildMonthlyAmountReward(user, midas, source, out NetRewardData monthlyReward))
            {
                return monthlyReward;
            }

            if (midas.ProductType == ProductType.CostumeShop &&
                TryBuildCostumeShopReward(user, midas, source, out NetRewardData costumeReward))
            {
                return costumeReward;
            }
        }

        return BuildPackageReward(user, packageShopId, packageListId, source);
    }

    public static NetRewardData BuildCampaignReward(User user, int packageGroupId, IEnumerable<int> obtainIds, string source)
    {
        NetRewardData reward = new() { PassPoint = new() };

        foreach (int obtainId in obtainIds)
        {
            if (!GameData.Instance.CampaignPackageGroupTable.TryGetValue(obtainId, out CampaignPackageGroupRecord? groupReward) ||
                groupReward.PackageGroupId != packageGroupId)
            {
                Logging.WriteLine(
                    $"{source} missing campaign reward packageGroupId={packageGroupId} obtainId={obtainId}",
                    LogType.Warning);
                continue;
            }

            RewardUtils.AddSingleObject(
                user,
                ref reward,
                groupReward.ProductId,
                groupReward.ProductType,
                groupReward.ProductValue);
        }

        return reward;
    }

    public static NetRewardData BuildPackageGroupReward(User user, int packageGroupId, string source)
    {
        NetRewardData reward = new() { PassPoint = new() };
        List<PackageGroupRecord> groupRewards = GameData.Instance.PackageGroupTable.Values
            .Where(item => item.PackageGroupId == packageGroupId)
            .OrderBy(item => item.Id)
            .ToList();

        if (groupRewards.Count == 0)
        {
            RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, 100);
            Logging.WriteLine(
                $"{source} fallback reward because packageGroupId={packageGroupId} had no PackageGroupTable rows",
                LogType.Warning);
            return reward;
        }

        foreach (PackageGroupRecord groupReward in groupRewards)
        {
            RewardUtils.AddSingleObject(
                user,
                ref reward,
                groupReward.ProductId,
                groupReward.ProductType,
                groupReward.ProductValue);
        }

        return reward;
    }

    public static NetRewardData BuildPackageReward(User user, int packageShopId, int packageListId, string source)
    {
        NetRewardData reward = new() { PassPoint = new() };

        PackageShopRecord? packageShop = null;
        PackageListRecord? packageList = null;
        if (packageShopId != 0)
        {
            GameData.Instance.PackageShopTable.TryGetValue(packageShopId, out packageShop);
        }

        if (packageShop == null && packageListId != 0 &&
            GameData.Instance.PackageListTable.TryGetValue(packageListId, out packageList))
        {
            if (!GameData.Instance.PackageShopTable.TryGetValue(packageList.PackageShopId, out packageShop))
            {
                GameData.Instance.PackageShopTable.TryGetValue(packageList.ProductId, out packageShop);
            }
        }

        if (packageShop == null && packageShopId != 0)
        {
            packageList ??= GameData.Instance.PackageListTable.Values
                .Where(item => item.PackageShopId == packageShopId && item.IsActive)
                .OrderBy(item => item.PackageOrder)
                .ThenBy(item => item.Id)
                .FirstOrDefault();

            if (packageList != null)
            {
                GameData.Instance.PackageShopTable.TryGetValue(packageList.ProductId, out packageShop);
            }
        }

        if (packageShop == null)
        {
            RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, 100);
            Logging.WriteLine(
            $"{source} fallback reward because packageShopId={packageShopId} packageListId={packageListId} packageProductId={packageList?.ProductId ?? 0} was not found",
            LogType.Warning);
            return reward;
        }

        List<PackageGroupRecord> groupRewards = GameData.Instance.PackageGroupTable.Values
            .Where(item => item.PackageGroupId == packageShop.PackageGroupId)
            .OrderBy(item => item.Id)
            .ToList();

        if (groupRewards.Count == 0)
        {
            RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, Math.Max(100, packageShop.ProductGrade * 100));
            Logging.WriteLine(
                $"{source} fallback reward because packageGroupId={packageShop.PackageGroupId} had no PackageGroupTable rows",
                LogType.Warning);
            return reward;
        }

        foreach (PackageGroupRecord groupReward in groupRewards)
        {
            RewardUtils.AddSingleObject(
                user,
                ref reward,
                groupReward.ProductId,
                groupReward.ProductType,
                groupReward.ProductValue);
        }

        return reward;
    }

    private static bool TryBuildCashShopReward(User user, MidasProductRecord midas, string source, out NetRewardData reward)
    {
        reward = new() { PassPoint = new() };

        CashShopRecord? cashShop = GameData.Instance.CashShopTable.Values.FirstOrDefault(item =>
            item.Id == midas.ProductId ||
            item.MidasProductId == midas.Id);

        if (cashShop == null)
        {
            Logging.WriteLine(
                $"{source} cash shop reward missing cash row productId={midas.MidasProductIdProximabeta} midasId={midas.Id} cashProductId={midas.ProductId}",
                LogType.Warning);
            return false;
        }

        switch (cashShop.ProductType)
        {
            case CashShopProductType.Currency:
                RewardUtils.AddSingleCurrencyObject(user, ref reward, (CurrencyType)cashShop.ProductId, cashShop.ProductValue);
                break;

            case CashShopProductType.Item:
                RewardUtils.AddSingleObject(user, ref reward, cashShop.ProductId, RewardType.Item, cashShop.ProductValue);
                break;

            case CashShopProductType.Package:
                reward = BuildPackageReward(user, cashShop.ProductId, 0, source);
                break;

            default:
                Logging.WriteLine(
                    $"{source} unsupported cash shop product type cashShopId={cashShop.Id} type={cashShop.ProductType}",
                    LogType.Warning);
                return false;
        }

        return true;
    }

    private static bool TryBuildCampaignPurchaseReward(User user, int packageGroupId, string source, out NetRewardData reward)
    {
        List<CampaignPackageGroupRecord> groupRewards = GameData.Instance.CampaignPackageGroupTable.Values
            .Where(item => item.PackageGroupId == packageGroupId && item.RewardType == CampaignPackageRewardType.Purchase)
            .OrderBy(item => item.Id)
            .ToList();

        if (groupRewards.Count == 0)
        {
            reward = new() { PassPoint = new() };
            Logging.WriteLine(
                $"{source} campaign purchase reward missing packageGroupId={packageGroupId}",
                LogType.Warning);
            return false;
        }

        reward = new() { PassPoint = new() };
        foreach (CampaignPackageGroupRecord groupReward in groupRewards)
        {
            RewardUtils.AddSingleObject(
                user,
                ref reward,
                groupReward.ProductId,
                groupReward.ProductType,
                groupReward.ProductValue);
        }

        return true;
    }

    private static bool TryBuildMonthlyAmountReward(User user, MidasProductRecord midas, string source, out NetRewardData reward)
    {
        MonthlyAmountRecord? monthly = GameData.Instance.MonthlyAmountTable.Values.FirstOrDefault(item =>
            item.Id == midas.ProductId ||
            item.MidasProductId == midas.Id);

        if (monthly == null)
        {
            reward = new() { PassPoint = new() };
            Logging.WriteLine(
                $"{source} monthly amount reward missing productId={midas.MidasProductIdProximabeta} midasId={midas.Id} monthlyProductId={midas.ProductId}",
                LogType.Warning);
            return false;
        }

        reward = BuildPackageGroupReward(user, monthly.BuyPackageGroupId, source);
        return true;
    }

    private static bool TryBuildCostumeShopReward(User user, MidasProductRecord midas, string source, out NetRewardData reward)
    {
        reward = new() { PassPoint = new() };
        CostumeShopRecord? costume = GameData.Instance.CostumeShopTable.Values.FirstOrDefault(item =>
            item.Id == midas.ProductId ||
            item.MidasProductId == midas.Id);

        if (costume == null)
        {
            Logging.WriteLine(
                $"{source} costume shop reward missing productId={midas.MidasProductIdProximabeta} midasId={midas.Id} costumeProductId={midas.ProductId}",
                LogType.Warning);
            return false;
        }

        foreach (PackageGroupRecord groupReward in GameData.Instance.PackageGroupTable.Values
            .Where(item => item.PackageGroupId == costume.PackageGroupId)
            .OrderBy(item => item.Id))
        {
            if (groupReward.ProductType == RewardType.CharacterCostume &&
                reward.CharacterCostume.Contains(groupReward.ProductId))
            {
                continue;
            }

            RewardUtils.AddSingleObject(
                user,
                ref reward,
                groupReward.ProductId,
                groupReward.ProductType,
                groupReward.ProductValue);
        }

        if (!reward.CharacterCostume.Contains(costume.CostumeId))
        {
            RewardUtils.AddSingleObject(user, ref reward, costume.CostumeId, RewardType.CharacterCostume, 1);
        }

        return true;
    }
}
