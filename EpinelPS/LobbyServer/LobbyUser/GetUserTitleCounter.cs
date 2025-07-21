using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitlecounter/get")]
    public class GetUserTitleCounter : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetUserTitleCounterList req = await ReadData<ReqGetUserTitleCounterList>();

            ResGetUserTitleCounterList r = new();

            await WriteDataAsync(r);
        }
    }
}
