using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Friend
{
    [PacketPath("/friend/get")]
    public class GetFriends : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFriendData>();
            var response = new ResGetFriendData();


            await WriteDataAsync(response);
        }
    }
}
