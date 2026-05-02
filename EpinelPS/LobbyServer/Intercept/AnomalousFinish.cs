namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/Finish")]
public class FinishAnomalousIntercept : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishInterceptAnomalous req = await ReadData<ReqFinishInterceptAnomalous>();

        ResFinishInterceptAnomalous response = new();

        await WriteDataAsync(response);
    }
}
