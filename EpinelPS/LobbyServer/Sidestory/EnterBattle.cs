namespace EpinelPS.LobbyServer.Sidestory;

[GameRequest("/sidestory/cut/enterbattle")]
public class EnterBattle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterSideStoryCutForBattle req = await ReadData<ReqEnterSideStoryCutForBattle>();
        User user = GetUser();

        ResEnterSideStoryCutForBattle response = new();

        await WriteDataAsync(response);
    }
}
