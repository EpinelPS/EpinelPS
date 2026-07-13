using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/renew")]
public class RenewShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopRenew req = await ReadData<ReqShopRenew>();
        ResShopRenew response = new()
        {
            Shop = new NetShopProductData()
        };
        User user = GetUser();

        try
        {
            NormalShopHelper.RenewShop(user, ref response, req.ShopCategory);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"Error renewing shop: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}
