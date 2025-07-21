using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitle/unmark-badge")]
    public class UnmarkUserTitleBase : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqUnMarkUserTitleBadge req = await ReadData<ReqUnMarkUserTitleBadge>();
            Database.User user = GetUser();

            ResUnMarkUserTitleBadge response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
