namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/bookmark/scenario/exist")]
public class CheckBookmarkScenarioExists : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExistScenarioBookmark req = await ReadData<ReqExistScenarioBookmark>();

        ResExistScenarioBookmark response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
