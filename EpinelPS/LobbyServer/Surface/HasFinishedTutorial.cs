namespace EpinelPS.LobbyServer.Surface;

[GameRequest("/Surface/HasFinishedTutorial")]
public class HasFinishedTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqHasFinishedSurfaceTutorial req = await ReadData<ReqHasFinishedSurfaceTutorial>();
        User user = GetUser();

        ResHasFinishedSurfaceTutorial response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
