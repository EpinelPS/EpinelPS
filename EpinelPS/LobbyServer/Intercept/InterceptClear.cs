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
                Intercept = req.Intercept,
                InterceptId = req.InterceptId,
                TicketCount = 5,
                MaxTicketCount = 10
            };

            await WriteDataAsync(response);
        }
    }
}
