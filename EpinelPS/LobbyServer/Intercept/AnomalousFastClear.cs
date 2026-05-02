namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/FastClear")]
public class AnomalousFastClear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearInterceptAnomalous req = await ReadData<ReqFastClearInterceptAnomalous>();

        ResFastClearInterceptAnomalous response = new();

        await WriteDataAsync(response);
    }
}
