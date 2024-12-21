using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/check")]
    public class CheckClearInterceptToday : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckClearInterceptToday>();

            var response = new ResCheckClearInterceptToday
            {
                Clear = true
            };

            await WriteDataAsync(response);
        }
    }
}
