namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/productlist")]
public class GetShopProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqShopProductList>();
        ResShopProductList response = new();
        response.Shops.AddRange(ShopCatalogBuilder.BuildContentShopBootList());

        await WriteDataAsync(response);
    }
}
