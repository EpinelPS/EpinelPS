using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/getfieldtalklist")]
    public class GetFieldTalkList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetFieldTalkList req = await ReadData<ReqGetFieldTalkList>();
            Database.User user = GetUser();

            ResGetFieldTalkList response = new();
            // TODO

            await WriteDataAsync(response);
        }
    }
}
