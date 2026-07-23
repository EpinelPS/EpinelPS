namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/setunittobuilding")]
public class LogSetUnitToBuilding : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogSetArcadeRebuildEdenUnitToBuilding req = await ReadData<ReqLogSetArcadeRebuildEdenUnitToBuilding>();
        User user = GetUser();
        ResLogSetArcadeRebuildEdenUnitToBuilding response = new();

        // TODO
        await WriteDataAsync(response);
    }
}