using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/renew")]
public class RenewShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopRenew req = await ReadData<ReqShopRenew>();
        User user = GetUser();

        Logging.WriteLine($"[Shop] /shop/renew called by user {user.Nickname}: ShopCategory={req.ShopCategory}", LogType.Debug);

        ResShopRenew response = new()
        {
            Shop = NormalShopHelper.GetShopData(req.ShopCategory),
        };

        await WriteDataAsync(response);
    }
}