namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/summary")]
public class LogSummary : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogArcadeRebuildEdenSummary req = await ReadData<ReqLogArcadeRebuildEdenSummary>();
        User user = GetUser();
        ResLogArcadeRebuildEdenSummary response = new();

        // TODO
        await WriteDataAsync(response);
    }
}