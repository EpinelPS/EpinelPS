using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/startpurchase")]
public class StartPurchase : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqStartPurchase req = await ReadData<ReqStartPurchase>();
        User user = GetUser();
        bool success = InAppPurchaseHelper.TrySimulatePurchase(user, req.ProductId, req.ExtraData, out _);

        var response = new ResStartPurchase
        {
            Result = success ? StartPurchaseResult.Ok : StartPurchaseResult.PayChannelNotAvailable,
            TransactionId = success ? $"dev-{Guid.NewGuid():N}" : string.Empty,
        };
        Logging.WriteLine($"[InAppShop] /inappshop/startpurchase product={req.ProductId}, simulated={success}", LogType.Info);
        await WriteDataAsync(response);
    }
}
