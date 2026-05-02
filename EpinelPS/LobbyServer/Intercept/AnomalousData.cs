namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/Data")]
public class GetAnomalousData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqInterceptAnomalousData req = await ReadData<ReqInterceptAnomalousData>();

        // TODO

        ResInterceptAnomalousData response = new()
        {
            InterceptAnomalousManagerId = 101,
            TodayRemainingTickets = 5
        };
        await WriteDataAsync(response);
    }
}
