namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/get")]
public class GetShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetShop x = await ReadData<ReqGetShop>();

        ResGetShop response = new()
        {
            Shop = new NetShopProductData
            {
                ShopCategory = x.ShopCategory
            }
        };

        // TODO

        await WriteDataAsync(response);
    }
}
