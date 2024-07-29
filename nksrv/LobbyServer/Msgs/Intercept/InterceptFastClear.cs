using nksrv.Net;
using nksrv.Utils;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/fastclear")]
    public class FastClearInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearIntercept>();

            var response = new ResFastClearIntercept
            {
                Intercept = 1,
                InterceptId = 1,
                TicketCount = 3,
                MaxTicketCount = 10
            };

            await WriteDataAsync(response);
        }
    }
}
