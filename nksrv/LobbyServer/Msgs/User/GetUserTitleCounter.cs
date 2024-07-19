using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/lobby/usertitlecounter/get")]
    public class GetUserTitleCounter : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetUserTitleCounterList>();

            var r = new ResGetUserTitleCounterList();

            await WriteDataAsync(r);
        }
    }
}
