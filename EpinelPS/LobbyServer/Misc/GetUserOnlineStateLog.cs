using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/useronlinestatelog")]
    public class GetUserOnlineStateLog : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqUserOnlineStateLog req = await ReadData<ReqUserOnlineStateLog>();
            User user = GetUser();

            ResUserOnlineStateLog response = new();
            user.LastLogin = DateTime.UtcNow;
            await WriteDataAsync(response);
        }
    }
}
