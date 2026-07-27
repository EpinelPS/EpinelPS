namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/buildbuilding")]
public class LogBuildBuilding : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogBuildArcadeRebuildEdenBuilding req = await ReadData<ReqLogBuildArcadeRebuildEdenBuilding>();
        User user = GetUser();
        ResLogBuildArcadeRebuildEdenBuilding response = new();

        // TODO
        await WriteDataAsync(response);
    }
}