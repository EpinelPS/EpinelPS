using System.Collections.Concurrent;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp;

internal static class InAppPurchaseHelper
{
    private static readonly ConcurrentDictionary<(ulong UserId, string ProductId), NetRewardData> PendingRewards = new();

    public static bool TrySimulatePurchase(User user, string productId, NetStartPurchaseExtraData? extraData,
        out NetRewardData reward)
    {
        reward = new NetRewardData { PassPoint = new NetPassPointData() };
        if (!GameConfig.Root.EnableFreeInAppPurchases)
        {
            Logging.WriteLine("[InAppShop] Simulated purchase rejected because EnableFreeInAppPurchases is disabled", LogType.Warning);
            return false;
        }

        var midas = FindMidasProduct(productId);
        if (midas == null || !midas.IsActive)
            return false;

        bool granted = midas.ProductType switch
        {
            ProductType.CashShop => GrantCashShop(user, midas.ProductId, ref reward),
            ProductType.PackageShop => GrantPackageShop(user, midas.ProductId, ref reward),
            ProductType.CostumeShop => GrantCostumeShop(user, midas.ProductId, ref reward),
            ProductType.PassCostumeShop => GrantPassCostumeShop(user, midas.ProductId, ref reward),
            _ => false,
        };

        if (!granted)
        {
            Logging.WriteLine($"[InAppShop] Unsupported simulated product {productId}: type={midas.ProductType}, tid={midas.ProductId}", LogType.Warning);
            return false;
        }

        PendingRewards[(user.ID, productId)] = reward.Clone();
        JsonDb.Save();
        Logging.WriteLine($"[InAppShop] Simulated free purchase for user {user.ID}: product={productId}, type={midas.ProductType}, tid={midas.ProductId}", LogType.Info);
        return true;
    }

    public static NetRewardData TakePendingReward(ulong userId, string productId)
    {
        return PendingRewards.TryRemove((userId, productId), out var reward)
            ? reward
            : new NetRewardData { IsEmptyReward = true, PassPoint = new NetPassPointData() };
    }

    private static MidasProductRecord? FindMidasProduct(string productId)
    {
        if (GameData.Instance.mediasProductTable.TryGetValue(productId, out var exact))
            return exact;

        return GameData.Instance.mediasProductTable.Values.FirstOrDefault(x =>
            string.Equals(x.MidasProductIdProximabeta, productId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.MidasProductIdGamamobi, productId, StringComparison.OrdinalIgnoreCase));
    }

    private static bool GrantCashShop(User user, int cashShopId, ref NetRewardData reward)
    {
        if (!GameData.Instance.CashShopRecords.TryGetValue(cashShopId, out var product) || !product.IsActive)
            return false;

        switch (product.ProductType)
        {
            case CashShopProductType.Currency:
                RewardUtils.AddSingleObject(user, ref reward, product.ProductId, RewardType.Currency, product.ProductValue);
                return true;
            case CashShopProductType.Item:
                RewardUtils.AddSingleObject(user, ref reward, product.ProductId, RewardType.Item, product.ProductValue);
                return true;
            case CashShopProductType.Package:
                return GrantPackageGroup(user, product.ProductId, ref reward);
            default:
                return false;
        }
    }

    private static bool GrantPackageShop(User user, int packageShopId, ref NetRewardData reward)
    {
        return GameData.Instance.PackageShopTable.TryGetValue(packageShopId, out var package) &&
               GrantPackageGroup(user, package.PackageGroupId, ref reward);
    }

    private static bool GrantCostumeShop(User user, int costumeShopId, ref NetRewardData reward)
    {
        if (!GameData.Instance.CostumeShopTable.TryGetValue(costumeShopId, out var costume) || !costume.IsActive)
            return false;

        AddCostume(user, costume.CostumeId, ref reward);
        return GrantPackageGroup(user, costume.PackageGroupId, ref reward, allowEmpty: true);
    }

    private static bool GrantPassCostumeShop(User user, int passCostumeShopId, ref NetRewardData reward)
    {
        if (!GameData.Instance.PassCostumeShopTable.TryGetValue(passCostumeShopId, out var costume))
            return false;

        AddCostume(user, costume.CostumeId, ref reward);
        return GrantPackageGroup(user, costume.PackageGroupId, ref reward, allowEmpty: true);
    }

    private static bool GrantPackageGroup(User user, int packageGroupId, ref NetRewardData reward, bool allowEmpty = false)
    {
        var products = GameData.Instance.PackageGroupTable.Values
            .Where(x => x.PackageGroupId == packageGroupId)
            .ToList();
        if (products.Count == 0)
            return allowEmpty;

        foreach (var product in products)
            RewardUtils.AddSingleObject(user, ref reward, product.ProductId, product.ProductType, product.ProductValue);
        return true;
    }

    private static void AddCostume(User user, int costumeId, ref NetRewardData reward)
    {
        if (!user.CostumeList.Contains(costumeId))
            user.CostumeList.Add(costumeId);
        reward.CharacterCostume.Add(costumeId);
    }
}
