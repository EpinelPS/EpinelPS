using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/buy")]
public class BuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyProduct req = await ReadData<ReqShopBuyProduct>();
        ResShopBuyProduct response = new();
        User user = GetUser();

        try
        {
            NormalShopHelper.BuyShopProduct(user, ref response, req);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"Error buying shop product: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}
