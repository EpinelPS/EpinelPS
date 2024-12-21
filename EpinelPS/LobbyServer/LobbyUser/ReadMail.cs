using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/mail/read")]
    public class ReadMail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqReadMail>();

            var r = new ResReadMail();
            //TODO
            await WriteDataAsync(r);
        }
    }
}
