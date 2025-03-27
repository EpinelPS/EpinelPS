using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/Data")]
    public class GetAnomalousData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqInterceptAnomalousData>();

            // TODO

            var response = new ResInterceptAnomalousData
            {
                InterceptAnomalousManagerId = 1,
                RemainingTickets = 5
            };
			response.ClearedInterceptAnomalousIds.Add(new[] { 1, 2, 3, 4, 5 });
            await WriteDataAsync(response);
        }
    }
}
