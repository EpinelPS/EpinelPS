namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/productlist")]
public class GetShopProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopProductList req = await ReadData<ReqShopProductList>();
        User user = GetUser();
        ResShopProductList response = new();

        var shops = NormalShopHelper.GetAllShopData(user);
        response.Shops.AddRange(shops);

        await WriteDataAsync(response);
    }
}
