using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/Enter")]
    public class EnterAnomalousIntercept : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterInterceptAnomalous>();

            var response = new ResEnterInterceptAnomalous();

            await WriteDataAsync(response);
        }
    }
}
