using EpinelPS.Data;
using System.Globalization;

namespace EpinelPS.LobbyServer.Shop.InApp;

internal static class PackagePurchaseResolver
{
    public static int ResolvePackageListId(string productId, int extraPackageListId = 0)
    {
        if (extraPackageListId != 0 && GameData.Instance.PackageListTable.ContainsKey(extraPackageListId))
        {
            return extraPackageListId;
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            return 0;
        }

        MidasProductRecord? midas = ResolveMidasProduct(productId);
        if (midas == null)
        {
            return 0;
        }

        return GameData.Instance.PackageListTable.Values
            .Where(item => item.ProductId == midas.ProductId)
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.Id)
            .FirstOrDefault()
            ?.Id ?? 0;
    }

    public static bool HasValidPackageProducts(InAppShopManagerRecord manager)
    {
        if (manager.PackageShopId == 0)
        {
            return true;
        }

        if (manager.ShopCategory == InAppShopCategory.CostumeShop ||
            manager.MainCategoryType == CashshopMainCategoryType.CostumeTab)
        {
            return GameData.Instance.CostumeShopTable.Values
                .Where(item => item.ShopGroupId == manager.PackageShopId && item.IsActive)
                .Any(item => FindPurchasableMidasByMidasId(item.MidasProductId) != null);
        }

        return GameData.Instance.PackageListTable.Values
            .Where(item => item.PackageShopId == manager.PackageShopId && item.IsActive)
            .Any(item => FindPurchasableMidasByProductId(item.ProductId) != null);
    }

    public static MidasProductRecord? ResolveMidasProduct(string productId)
    {
        return GameData.Instance.mediasProductTable.Values
            .FirstOrDefault(item =>
                item.MidasProductIdProximabeta == productId ||
                item.MidasProductIdGamamobi == productId);
    }

    private static MidasProductRecord? FindPurchasableMidasByProductId(int productId)
    {
        return GameData.Instance.mediasProductTable.Values.FirstOrDefault(item =>
            item.ProductId == productId &&
            item.IsActive &&
            !item.IsFree &&
            !string.IsNullOrWhiteSpace(item.Cost) &&
            decimal.TryParse(item.Cost.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _));
    }

    private static MidasProductRecord? FindPurchasableMidasByMidasId(int midasId)
    {
        return GameData.Instance.mediasProductTable.Values.FirstOrDefault(item =>
            item.Id == midasId &&
            item.IsActive &&
            !item.IsFree &&
            !string.IsNullOrWhiteSpace(item.Cost) &&
            decimal.TryParse(item.Cost.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _));
    }
}
