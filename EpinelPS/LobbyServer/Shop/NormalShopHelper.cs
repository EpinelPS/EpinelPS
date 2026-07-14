using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

public static class NormalShopHelper
{
    public static List<NetShopProductData> GetAllShopData()
    {
        var shops = new List<NetShopProductData>();

        var shop = new NetShopProductData
        {
            ShopTid = 0,
            ShopCategory = 1,
        };

        shops.Add(shop);
        return shops;
    }

    public static NetShopProductData GetShopData(int shopCategory)
    {
        var shops = GetAllShopData();
        return shops.FirstOrDefault(s => s.ShopCategory == shopCategory) ?? new NetShopProductData();
    }
}
