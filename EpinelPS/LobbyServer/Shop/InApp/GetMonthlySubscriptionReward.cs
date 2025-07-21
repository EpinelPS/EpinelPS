using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp
{
    [PacketPath("/inappshop/getmonthlysubscriptionreward")]
    public class GetMonthlySubscriptionReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMonthlySubscriptionReward req = await ReadData<ReqGetMonthlySubscriptionReward>();

            ResGetMonthlySubscriptionReward response = new();

            // TODO: Validate response from real server
            await WriteDataAsync(response);
        }
    }
}
