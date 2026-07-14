using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/get")]
public class GetShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetShop req = await ReadData<ReqGetShop>();
        User user = GetUser();

        Logging.WriteLine($"[Shop] /shop/get called by user {user.Nickname}, ShopCategory={req.ShopCategory}", LogType.Debug);

        ResGetShop response = new()
        {
            Shop = NormalShopHelper.GetShopData(req.ShopCategory)
        };

        await WriteDataAsync(response);
    }
}
