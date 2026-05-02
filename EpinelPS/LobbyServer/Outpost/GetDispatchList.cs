namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/dispatch/get")]
public class GetDispatchList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetDispatchList req = await ReadData<ReqGetDispatchList>();

        ResGetDispatchList response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
