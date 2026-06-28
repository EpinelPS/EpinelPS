using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop;

internal static class ShopCatalogBuilder
{
    public static string DescribeContentShopHeaders(int limit = 40)
    {
        Dictionary<int, int> productCountByBundle = GameData.Instance.ContentsShopProductTable.Values
            .GroupBy(product => product.BundleId)
            .ToDictionary(group => group.Key, group => group.Count());

        return string.Join(",", GameData.Instance.ContentsShopTable.Values
            .Where(shop => shop.ShopType == ShopType.MainShop)
            .OrderBy(shop => shop.ShopCategory)
            .ThenBy(shop => shop.Id)
            .Take(limit)
            .Select(shop =>
            {
                productCountByBundle.TryGetValue(shop.BundleId, out int productCount);
                return $"{(int)shop.ShopCategory}:{shop.Id}:bundle={shop.BundleId}:staticProducts={productCount}";
            }));
    }

    public static IEnumerable<NetShopProductData> BuildContentShopHeaders()
    {
        IEnumerable<ContentsShopRecord> shops = GameData.Instance.ContentsShopTable.Values
            .Where(shop => shop.ShopType == ShopType.MainShop)
            .OrderBy(shop => shop.ShopCategory)
            .ThenBy(shop => shop.Id);

        foreach (ContentsShopRecord shop in shops)
        {
            yield return new NetShopProductData
            {
                ShopTid = shop.Id,
                ShopCategory = (int)shop.ShopCategory,
                RenewAt = DateTime.Now.AddDays(-1).Ticks,
                NextRenewAt = GetNextRenewAt(shop)
            };
        }
    }

    public static IEnumerable<NetShopProductData> BuildContentShopBootList()
    {
        HashSet<int> populatedCategories = [(int)ShopCategoryType.ShopNormal];

        foreach (NetShopProductData header in BuildContentShopHeaders())
        {
            if (!populatedCategories.Contains(header.ShopCategory))
            {
                yield return header;
                continue;
            }

            NetShopProductData? populated = BuildContentShop(header.ShopCategory, header.ShopTid, maxProducts: 4);
            yield return populated ?? header;
        }
    }

    public static IEnumerable<NetShopProductData> BuildContentShops(int? category = null)
    {
        IEnumerable<ContentsShopRecord> shops = GameData.Instance.ContentsShopTable.Values
            .Where(shop => shop.ShopType == ShopType.MainShop)
            .Where(shop => category == null || (int)shop.ShopCategory == category.Value)
            .OrderBy(shop => shop.ShopCategory)
            .ThenBy(shop => shop.Id);

        foreach (ContentsShopRecord shop in shops)
        {
            NetShopProductData? data = BuildContentShop((int)shop.ShopCategory, shop.Id);
            if (data != null)
            {
                yield return data;
            }
        }
    }

    private static NetShopProductData? BuildContentShop(int category, int shopTid, int maxProducts = 16)
    {
        ContentsShopRecord? shop = GameData.Instance.ContentsShopTable.Values
            .FirstOrDefault(candidate =>
                candidate.ShopType == ShopType.MainShop &&
                candidate.Id == shopTid &&
                (int)candidate.ShopCategory == category);

        if (shop == null)
        {
            return null;
        }

        Dictionary<int, List<ContentsShopProductRecord>> productsByBundle = GameData.Instance.ContentsShopProductTable.Values
            .GroupBy(product => product.BundleId)
            .ToDictionary(group => group.Key, group => group.OrderBy(product => product.ProductOrder).ThenBy(product => product.Id).ToList());

        if (!productsByBundle.TryGetValue(shop.BundleId, out List<ContentsShopProductRecord>? products) || products.Count == 0)
        {
            return null;
        }

        NetShopProductData data = new()
        {
            ShopTid = shop.Id,
            ShopCategory = (int)shop.ShopCategory,
            RenewAt = DateTime.Now.AddDays(-1).Ticks,
            NextRenewAt = GetNextRenewAt(shop)
        };

        List<ContentsShopProductRecord> displayProducts = products.Take(maxProducts).ToList();
        bool hasDuplicateOrders = displayProducts
            .GroupBy(product => product.ProductOrder)
            .Any(group => group.Count() > 1);

        foreach ((ContentsShopProductRecord product, int index) in displayProducts.Select((product, index) => (product, index)))
        {
            data.List.Add(new NetShopProductInfoData()
            {
                Order = hasDuplicateOrders ? index + 1 : product.ProductOrder,
                ProductId = product.Id,
                BuyLimitCount = product.IsLimit ? product.BuyLimitCount : 0,
                BuyCount = 0,
                Discount = GetDiscount(product.DiscountProbId),
                EndAt = data.NextRenewAt,
                UseDateCondition = false
            });
        }

        return data;
    }

    private static long GetNextRenewAt(ContentsShopRecord shop)
    {
        DateTime now = DateTime.Now;
        return shop.RenewType switch
        {
            RenewType.AutoDay => now.Date.AddDays(1).Ticks,
            RenewType.AutoWeek => now.Date.AddDays(7).Ticks,
            RenewType.AutoMonth => now.Date.AddMonths(1).Ticks,
            _ => now.AddDays(30).Ticks
        };
    }

    private static int GetDiscount(int discountProbId)
    {
        if (discountProbId == 0)
        {
            return 0;
        }

        return GameData.Instance.ShopDiscountProbTable.TryGetValue(discountProbId, out ShopDiscountProbRecord? discount)
            ? discount.DiscountRate
            : 0;
    }
}
