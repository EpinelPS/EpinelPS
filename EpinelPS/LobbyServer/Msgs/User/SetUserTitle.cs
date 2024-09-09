using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/lobby/usertitle/set")]
    public class SetUserTitleData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetUserTitle>();

            var response = new ResSetUserTitle();

            await WriteDataAsync(response);
        }
    }
}
