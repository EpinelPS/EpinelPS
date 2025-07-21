using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/Finish")]
    public class FinishAnomalousIntercept : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFinishInterceptAnomalous req = await ReadData<ReqFinishInterceptAnomalous>();

            ResFinishInterceptAnomalous response = new();

            await WriteDataAsync(response);
        }
    }
}
