namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/getclearinfo")]
public class GetInterceptClearInfo : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetInterceptClearInfo>();
        ResGetInterceptClearInfo response = new();
        await WriteDataAsync(response);
    }
}
