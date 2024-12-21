using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/Finish")]
    public class FinishAnomalousIntercept : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFinishInterceptAnomalous>();

			var response = new ResFinishInterceptAnomalous();

            await WriteDataAsync(response);
        }
    }
}
