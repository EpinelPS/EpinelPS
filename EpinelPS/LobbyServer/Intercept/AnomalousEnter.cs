namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/Enter")]
public class EnterAnomalousIntercept : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterInterceptAnomalous req = await ReadData<ReqEnterInterceptAnomalous>();

        ResEnterInterceptAnomalous response = new();

        await WriteDataAsync(response);
    }
}
