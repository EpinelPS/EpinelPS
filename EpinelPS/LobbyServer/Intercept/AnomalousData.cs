using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/Data")]
    public class GetAnomalousData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqInterceptAnomalousData req = await ReadData<ReqInterceptAnomalousData>();

            // TODO

            ResInterceptAnomalousData response = new()
            {
                InterceptAnomalousManagerId = 101,
                TodayRemainingTickets = 5
            };
            await WriteDataAsync(response);
        }
    }
}
