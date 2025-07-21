using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/mail/read")]
    public class ReadMail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqReadMail req = await ReadData<ReqReadMail>();

            ResReadMail r = new();
            //TODO
            await WriteDataAsync(r);
        }
    }
}
