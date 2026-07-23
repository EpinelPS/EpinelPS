namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/upgradebuilding")]
public class LogUpgradeBuilding : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogUpgradeArcadeRebuildEdenBuilding req = await ReadData<ReqLogUpgradeArcadeRebuildEdenBuilding>();
        User user = GetUser();
        ResLogUpgradeArcadeRebuildEdenBuilding response = new();

        // TODO
        await WriteDataAsync(response);
    }
}