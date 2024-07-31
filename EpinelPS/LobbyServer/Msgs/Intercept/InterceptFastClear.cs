using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/fastclear")]
    public class FastClearInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearIntercept>();

            var response = new ResFastClearIntercept
            {
                TicketCount = 3,
                MaxTicketCount = 10,
                Damage = 2
            };

            await WriteDataAsync(response);
        }
    }
}
