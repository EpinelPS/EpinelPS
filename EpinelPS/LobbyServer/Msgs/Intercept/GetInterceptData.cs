using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/get")]
    public class GetInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetInterceptData>();

            var response = new ResGetInterceptData
            {
                NormalInterceptGroup = 1,
                SpecialInterceptId = 1,
                TicketCount = 5,
                MaxTicketCount = 10
            };

            await WriteDataAsync(response);
        }
    }
}
