using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/productlist")]
public class GetShopProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopProductList req = await ReadData<ReqShopProductList>();
        User user = GetUser();

        ResShopProductList response = new();
        response.Shops.AddRange(NormalShopHelper.GetAllShopData(user));

        await WriteDataAsync(response);
    }
}
