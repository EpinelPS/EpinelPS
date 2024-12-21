using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Auth
{
    [PacketPath("/auth/logout")]
    public class AuthLogout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqLogout>();

            // TODO remove UsedAuthToken

            await WriteDataAsync(new ResLogout());
        }
    }
}
