using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/lobby/usertitle/get")]
    public class GetUserTitle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetUserTitleList>();

            var r = new ResGetUserTitleList();
            r.UserTitleList.Add(new NetUserTitle() { UserTitleId = 1 });

            await WriteDataAsync(r);
        }
    }
}
