using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/proceed")]
    public class ProceedMsg : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqProceedMessage>();

            // TODO: save these things
            var response = new ResProceedMessage();

            await WriteDataAsync(response);
        }
    }
}
