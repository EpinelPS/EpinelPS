namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/startpurchase")]
public class StartPurchase : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqStartPurchase req = await ReadData<ReqStartPurchase>();
        User user = GetUser();
        var midas = InAppPurchaseHelper.FindMidasProduct(req.ProductId);
        bool success = midas != null && midas.IsActive;

        var response = new ResStartPurchase
        {
            Result = success ? StartPurchaseResult.Ok : StartPurchaseResult.PayChannelNotAvailable,
            TransactionId = success ? $"dev-{Guid.NewGuid():N}" : string.Empty,
        };
        await WriteDataAsync(response);
    }
}
