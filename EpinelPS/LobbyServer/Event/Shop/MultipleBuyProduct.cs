using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Shop;

[GameRequest("/event/shopmultiplebuyproduct")]
public class MultipleBuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // ReqEventShopMultipleBuyProduct Fileds
        //  int EventId
        //  int ShopCategory
        //  RepeatedField<NetBuyProductRequestData> Products
        // NetBuyProductRequestData Fileds
        //  int ShopProductTid
        //  int Order
        //  int Quantity
        var req = await ReadData<ReqEventShopMultipleBuyProduct>();

        // ResEventShopMultipleBuyProduct Fileds
        //  EventShopBuyProductResult Result
        //  NetShopBuyMultipleProductData Product
        //  RepeatedField<NetUserItemData> Items
        //  RepeatedField<NetUserCurrencyData> Currencies
        ResEventShopMultipleBuyProduct response = new()
        {
            Result = EventShopBuyProductResult.Success
        };
        User user = GetUser();

        try
        {
            EventShopHelper.BuyShopMultipleProduct(user, ref response, req);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"Error buying shop product: {ex.Message}", LogType.Error);
        }



        await WriteDataAsync(response);
    }
}
