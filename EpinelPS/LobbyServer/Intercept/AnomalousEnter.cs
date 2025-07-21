using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/Enter")]
    public class EnterAnomalousIntercept : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterInterceptAnomalous req = await ReadData<ReqEnterInterceptAnomalous>();

            ResEnterInterceptAnomalous response = new();

            await WriteDataAsync(response);
        }
    }
}
