using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/check")]
    public class CheckClearInterceptToday : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckClearInterceptToday req = await ReadData<ReqCheckClearInterceptToday>();

            ResCheckClearInterceptToday response = new()
            {
                Clear = true
            };

            await WriteDataAsync(response);
        }
    }
}
