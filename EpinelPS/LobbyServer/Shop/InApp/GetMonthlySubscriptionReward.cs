namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getmonthlysubscriptionreward")]
public class GetMonthlySubscriptionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMonthlySubscriptionReward req = await ReadData<ReqGetMonthlySubscriptionReward>();

        ResGetMonthlySubscriptionReward response = new();

        // TODO: ValIdate response from real server
        await WriteDataAsync(response);
    }
}
