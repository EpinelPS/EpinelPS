using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/useronlinestatelog")]
    public class GetUserOnlineStateLog : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqUserOnlineStateLog>();
            var user = GetUser();

            var response = new ResUserOnlineStateLog();
            user.LastLogin = DateTime.UtcNow;
            await WriteDataAsync(response);
        }
    }
}
