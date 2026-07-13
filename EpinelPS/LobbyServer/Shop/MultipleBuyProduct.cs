using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/multiple-buy")]
public class MultipleBuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyMultipleProduct req = await ReadData<ReqShopBuyMultipleProduct>();
        ResShopBuyMultipleProduct response = new()
        {
            Product = new NetShopBuyMultipleProductData(),
            Result = ShopBuyProductResult.Success
        };
        User user = GetUser();

        try
        {
            NormalShopHelper.BuyShopMultipleProduct(user, ref response, req);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"Error buying multiple shop products: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}
