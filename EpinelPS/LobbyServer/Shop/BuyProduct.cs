using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/buy")]
public class BuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyProduct req = await ReadData<ReqShopBuyProduct>();
        User user = GetUser();

        Logging.WriteLine($"[Shop] /shop/buy called by user {user.Nickname}: ShopCategory={req.ShopCategory}, Order={req.Order}, ProductTid={req.ShopProductTid}, Qty={req.Quantity}", LogType.Debug);

        ResShopBuyProduct response = new();

        await WriteDataAsync(response);
    }
}
