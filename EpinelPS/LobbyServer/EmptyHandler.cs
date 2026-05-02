namespace EpinelPS.LobbyServer;

public class EmptyHandler : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetNow req = await ReadData<ReqGetNow>();
        ResGetNow response = new();

        await WriteDataAsync(response);
    }
}
