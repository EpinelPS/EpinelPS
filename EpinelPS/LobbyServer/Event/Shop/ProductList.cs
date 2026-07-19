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

            // Initialize ShopNormal's ShopCategoryData to prevent client errors.
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
