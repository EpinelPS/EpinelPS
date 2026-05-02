namespace EpinelPS.LobbyServer.Sidestory;

[GameRequest("/sidestory/cut/clearscenario")]
public class ClearSideStoryCut : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearSideStoryCutForScenario req = await ReadData<ReqClearSideStoryCutForScenario>();
        User user = GetUser();

        ResClearSideStoryCutForScenario response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
