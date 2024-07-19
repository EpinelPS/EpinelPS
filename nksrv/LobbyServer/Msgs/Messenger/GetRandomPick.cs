using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/random/pick")]
    public class GetRandomPick : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqForcePickTodayRandomMessage>();

            // TODO: get proper response
            var response = new ResForcePickTodayRandomMessage();

            await WriteDataAsync(response);
        }
    }
}
