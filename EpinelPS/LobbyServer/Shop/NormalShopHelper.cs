using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

public static class NormalShopHelper
{
    public static List<NetShopProductData> GetAllShopData()
    {
        var result = new List<NetShopProductData>();

        foreach (var shop in GameData.Instance.ContentsShopTable.Values
                     .Where(s => s.ShopType == ShopType.MainShop)
                     // Several records describe later unlock stages of the same category.
                     // The client stores one ShopCategoryData per ShopCategoryType.
                     .GroupBy(s => s.ShopCategory)
                     .Select(g => g.OrderBy(s => s.Id).First())
                     .OrderBy(s => s.Id))
        {
            var data = BuildContentsShopData(shop);
            if (data.List.Count > 0)
                result.Add(data);
        }

        return result;
    }

    public static NetShopProductData GetShopData(int shopCategory)
    {
        var shop = GameData.Instance.ContentsShopTable.Values.FirstOrDefault(s =>
            s.ShopType == ShopType.MainShop && (int)s.ShopCategory == shopCategory);
        if (shop == null)
            return new NetShopProductData { ShopCategory = shopCategory };

        return BuildContentsShopData(shop);
    }

    private static NetShopProductData BuildContentsShopData(ContentsShopRecord shop)
    {
        var now = DateTime.UtcNow;
        var result = new NetShopProductData
        {
            ShopTid = shop.Id,
            ShopCategory = (int)shop.ShopCategory,
            RenewCount = 0,
            RenewAt = GetRenewAt(shop, now),
            NextRenewAt = GetNextRenewAt(shop, now),
            FreeRenewCount = 1,
        };

        var products = GameData.Instance.ContentsShopProductTable.Values
            .Where(p => p.BundleId == shop.BundleId)
            // A product order is a single slot. The static table contains
            // weighted alternatives for that slot (ProductProb), but the
            // client indexes the wire list by Order and rejects duplicates.
            .GroupBy(p => p.ProductOrder)
            .Select(g => g.OrderByDescending(p => p.ProductProb).ThenBy(p => p.Id).First())
            .OrderBy(p => p.ProductOrder);

        foreach (var product in products)
        {
            result.List.Add(new NetShopProductInfoData
            {
                Order = product.ProductOrder,
                ProductId = product.Id,
                BuyLimitCount = product.IsLimit ? product.BuyLimitCount : 0,
                BuyCount = 0,
            });
        }

        return result;
    }

    private static long GetRenewAt(ContentsShopRecord shop, DateTime now)
    {
        if (shop.RenewType == RenewType.None)
            return 0;

        return now.Date.Ticks;
    }

    private static long GetNextRenewAt(ContentsShopRecord shop, DateTime now)
    {
        // Renewal timestamps are part of the shop state. Returning `now +
        // period` makes every product-list response different and causes the
        // client to immediately request /shop/productlist again in a loop.
        // Use a stable UTC boundary instead.
        var days = shop.RenewType switch
        {
            RenewType.AutoDay => Math.Max(1, shop.RenewValue),
            RenewType.AutoWeek => Math.Max(1, shop.RenewValue) * 7,
            RenewType.AutoMonth => Math.Max(1, shop.RenewValue) * 30,
            _ => 0
        };

        return days == 0 ? 0 : now.Date.AddDays(days).Ticks;
    }

    public static ShopProductRecord? GetProductById(int productId)
    {
        return GameData.Instance.ShopProductTable.TryGetValue(productId, out var product) ? product : null;
    }
}
