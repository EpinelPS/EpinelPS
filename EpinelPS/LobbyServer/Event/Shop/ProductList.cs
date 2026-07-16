using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Shop;

[GameRequest("/event/shopproductlist")]
public class ProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // int EventId
        var req = await ReadData<ReqEventShopProductList>();
        ResEventShopProductList response = new();
        User user = GetUser();

        try
        {
            response.Shops.Add(EventShopHelper.InitShopData(user, req.EventId));

            // 初始化 ShopNormal 的 ShopCategoryData，防止客户端找不到而崩溃
            if (!GameData.Instance.ShopTable.Values.Any(s => (int)s.ShopCategory == 1))
            {
                response.Shops.Add(new NetEventShopProductData
                {
                    ShopTid = 1,
                    ShopCategory = 1,
                });
            }
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"Get EventShopProductList Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}
