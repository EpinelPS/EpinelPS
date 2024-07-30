using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/get")]
    public class GetMessages : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMessages>();

            // TODO: save these things
            var response = new ResGetMessages();

            await WriteDataAsync(response);
        }
    }
}
