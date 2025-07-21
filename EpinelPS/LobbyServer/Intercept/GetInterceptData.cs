using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/get")]
    public class GetInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetInterceptData req = await ReadData<ReqGetInterceptData>();

            ResGetInterceptData response = new()
            {
                NormalInterceptGroup = 1,
                SpecialInterceptId = 1, // TODO switch this out each reset
                TicketCount = User.ResetableData.InterceptionTickets,
                MaxTicketCount = JsonDb.Instance.MaxInterceptionCount
            };

            await WriteDataAsync(response);
        }
    }
}
