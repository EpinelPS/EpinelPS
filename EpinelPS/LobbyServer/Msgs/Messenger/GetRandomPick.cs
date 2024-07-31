using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/random/pick")]
    public class GetRandomPick : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqPickTodayRandomMessage>();

            // TODO: get proper response
            var response = new ResPickTodayRandomMessage();

            await WriteDataAsync(response);
        }
    }
}
