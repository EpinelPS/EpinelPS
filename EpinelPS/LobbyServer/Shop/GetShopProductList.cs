namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/productlist")]
public class GetShopProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopProductList req = await ReadData<ReqShopProductList>();
        ResShopProductList response = new();

        await WriteDataAsync(response);
    }
}
