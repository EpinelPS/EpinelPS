using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/Anomalous/FastClear")]
    public class AnomalousFastClear : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearInterceptAnomalous req = await ReadData<ReqFastClearInterceptAnomalous>();

            ResFastClearInterceptAnomalous response = new();

            await WriteDataAsync(response);
        }
    }
}
