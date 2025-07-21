using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Friend
{
    [PacketPath("/friend/get")]
    public class GetFriends : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetFriendData req = await ReadData<ReqGetFriendData>();
            ResGetFriendData response = new();


            await WriteDataAsync(response);
        }
    }
}
