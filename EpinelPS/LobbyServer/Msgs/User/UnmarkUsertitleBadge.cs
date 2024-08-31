using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/lobby/usertitle/unmark-badge")]
    public class UnmarkUserTitleBase : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqUnMarkUserTitleBadge>();
            var user = GetUser();

            var response = new ResUnMarkUserTitleBadge();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
