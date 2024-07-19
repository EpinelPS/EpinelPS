using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/getsubscription")]
    public class GetInAppSubscription : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSubscription>();

            var response = new ResGetSubscription();

            // TODO
            await WriteDataAsync(response);
        }
    }
}
