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
                RemainingTickets = 5
            };
			response.ClearedInterceptAnomalousIds.Add([1, 2, 3, 4, 5]);
            await WriteDataAsync(response);
        }
    }
}
