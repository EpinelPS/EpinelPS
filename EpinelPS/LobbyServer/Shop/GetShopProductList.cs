using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/productlist")]
public class GetShopProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopProductList req = await ReadData<ReqShopProductList>();
        ResShopProductList response = new();

        var shops = NormalShopHelper.GetAllShopData();
        response.Shops.AddRange(shops);

        Logging.WriteLine($"[Shop] /shop/productlist returning {shops.Count} shops, category={shops.FirstOrDefault()?.ShopCategory}", LogType.Debug);

        await WriteDataAsync(response);
    }
}
