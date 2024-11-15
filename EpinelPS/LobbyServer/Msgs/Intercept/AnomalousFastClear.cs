using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/Anomalous/FastClear")]
    public class AnomalousFastClear : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearInterceptAnomalous>();

            var response = new ResFastClearInterceptAnomalous();

            await WriteDataAsync(response);
        }
    }
}
