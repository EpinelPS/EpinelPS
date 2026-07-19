using EpinelPS.Database;
namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/renew")]
public class RenewShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopRenew req = await ReadData<ReqShopRenew>();
        User user = GetUser();

        ResShopRenew response = new()
        {
            Shop = NormalShopHelper.GetShopData(user, req.ShopCategory, reroll: true),
        };

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
