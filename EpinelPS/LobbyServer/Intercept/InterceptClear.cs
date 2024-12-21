using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/clear")]
    public class ClearInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearIntercept>();

            var response = new ResClearIntercept
            {
                Intercept = 1,
                InterceptId = 1,
                TicketCount = 5,
                MaxTicketCount = 10
            };

            await WriteDataAsync(response);
        }
    }
}
