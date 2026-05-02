namespace EpinelPS.LobbyServer.Surface;

[GameRequest("/surface/lobby/simpledata")]
public class GetSimpleData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLobbySurfaceSimpleData req = await ReadData<ReqLobbySurfaceSimpleData>();
        User user = GetUser();

        ResLobbySurfaceSimpleData response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
