using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/getmonthlysubscriptionreward")]
    public class GetMonthlySubscriptionReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMonthlySubscriptionReward>();

            var response = new ResGetMonthlySubscriptionReward();

            // TODO: Validate response from real server
            await WriteDataAsync(response);
        }
    }
}
