namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/hireunit")]
public class LogHireUnit : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogHireArcadeRebuildEdenUnit req = await ReadData<ReqLogHireArcadeRebuildEdenUnit>();
        User user = GetUser();
        ResLogHireArcadeRebuildEdenUnit response = new();

        // TODO
        await WriteDataAsync(response);
    }
}