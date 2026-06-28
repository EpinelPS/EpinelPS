namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/get")]
public class GetShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetShop x = await ReadData<ReqGetShop>();

        ResGetShop response = new()
        {
            Shop = ShopCatalogBuilder.BuildContentShops(x.ShopCategory).FirstOrDefault()
                ?? new NetShopProductData { ShopCategory = x.ShopCategory }
        };

        await WriteDataAsync(response);
    }
}
