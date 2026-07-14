using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/multiple-buy")]
public class BuyMultipleProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyMultipleProduct req = await ReadData<ReqShopBuyMultipleProduct>();
        User user = GetUser();

        string details = string.Join(", ", req.Products.Select(p => $"Tid={p.ShopProductTid},Order={p.Order},Qty={p.Quantity}"));
        Logging.WriteLine($"[Shop] /shop/multiple-buy called by user {user.Nickname}: ShopCategory={req.ShopCategory}, Products=[{details}]", LogType.Debug);

        ResShopBuyMultipleProduct response = new();

        await WriteDataAsync(response);
    }
}
