namespace EpinelPS.LobbyServer.Sidestory;

[GameRequest("/sidestory/cut/clearbattle")]
public class ClearBattle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearSideStoryCutForBattle req = await ReadData<ReqClearSideStoryCutForBattle>();
        User user = GetUser();

        ResClearSideStoryCutForBattle response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
