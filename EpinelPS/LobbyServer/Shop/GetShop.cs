using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/get")]
public class GetShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetShop req = await ReadData<ReqGetShop>();
        User user = GetUser();

        ResGetShop response = new()
        {
            Shop = NormalShopHelper.GetShopData(user, req.ShopCategory)
        };

        await WriteDataAsync(response);
    }
}
