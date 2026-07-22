namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/destroybuilding")]
public class LogDestroyBuilding : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogDestroyArcadeRebuildEdenBuilding req = await ReadData<ReqLogDestroyArcadeRebuildEdenBuilding>();
        User user = GetUser();
        ResLogDestroyArcadeRebuildEdenBuilding response = new();

        await WriteDataAsync(response);
    }
}