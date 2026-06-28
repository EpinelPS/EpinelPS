namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getmonthlysubscriptionreward")]
public class GetMonthlySubscriptionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMonthlySubscriptionReward req = await ReadData<ReqGetMonthlySubscriptionReward>();

        ResGetMonthlySubscriptionReward response = new();

        bool changed = InAppPurchaseStateService.AppendMonthlySubscriptionData(
            User,
            response,
            "InAppShop/getmonthlysubscriptionreward");

        if (changed)
        {
            EpinelPS.Database.JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
