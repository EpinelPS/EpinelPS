namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/check")]
public class CheckClearInterceptToday : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckClearInterceptToday req = await ReadData<ReqCheckClearInterceptToday>();

        ResCheckClearInterceptToday response = new()
        {
            Clear = true
        };

        await WriteDataAsync(response);
    }
}
