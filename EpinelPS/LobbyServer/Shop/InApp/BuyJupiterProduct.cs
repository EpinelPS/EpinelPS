using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/jupiter/buyproduct")]
public class BuyJupiterProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqBuyJupiterProduct req = await ReadData<ReqBuyJupiterProduct>();
        User user = GetUser();
        bool success = InAppPurchaseHelper.TrySimulatePurchase(user, req.ProductId, req.ExtraData, out _);
        string referenceId = success ? $"dev-{Guid.NewGuid():N}" : string.Empty;

        Logging.WriteLine($"[InAppShop] /inappshop/jupiter/buyproduct product={req.ProductId}, simulated={success}", LogType.Info);
        await WriteDataAsync(new ResBuyJupiterProduct
        {
            ReferenceId = referenceId,
            RedirectUrl = string.Empty,
        });
    }
}
