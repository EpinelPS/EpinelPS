namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/startpurchase")]
public class StartPurchase : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqStartPurchase req = await ReadData<ReqStartPurchase>();
        string transactionId = $"epinel-{req.ProductId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        ResStartPurchase response = new()
        {
            Result = StartPurchaseResult.Ok,
            TransactionId = transactionId
        };

        await WriteDataAsync(response);
    }
}
