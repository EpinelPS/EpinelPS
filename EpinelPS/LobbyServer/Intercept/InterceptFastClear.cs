using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/fastclear")]
    public class FastClearInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearIntercept req = await ReadData<ReqFastClearIntercept>();

            ResFastClearIntercept response = new()
            {
                TicketCount = User.ResetableData.InterceptionTickets,
                MaxTicketCount = JsonDb.Instance.MaxInterceptionCount,
                Damage = 0
            };

            await WriteDataAsync(response);
        }
    }
}
