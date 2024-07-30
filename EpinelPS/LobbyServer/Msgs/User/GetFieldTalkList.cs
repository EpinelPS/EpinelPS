using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/user/getfieldtalklist")]
    public class GetFieldTalkList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFieldTalkList>();
            var user = GetUser();

            var response = new ResGetFieldTalkList();
            // TODO

            await WriteDataAsync(response);
        }
    }
}
