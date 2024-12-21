using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
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
