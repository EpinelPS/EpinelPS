using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/enterlobbyping")]
    public class EnterLobbyPing : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterLobbyPing>();

            var response = new ResEnterLobbyPing();

          await  WriteDataAsync(response);
        }
    }
}
