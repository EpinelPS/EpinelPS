using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Auth
{
    [PacketPath("/auth/logout")]
    public class AuthLogout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqLogout>();

            JsonDb.Instance.GameClientTokens.Remove(UsedAuthToken);

            await WriteDataAsync(new ResLogout());
        }
    }
}
