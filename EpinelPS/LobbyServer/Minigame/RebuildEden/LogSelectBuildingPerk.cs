namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/selectbuildingperk")]
public class LogSelectBuildingPerk : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogSelectArcadeRebuildEdenBuildingPerk req = await ReadData<ReqLogSelectArcadeRebuildEdenBuildingPerk>();
        User user = GetUser();
        ResLogSelectArcadeRebuildEdenBuildingPerk response = new();

        // TODO
        await WriteDataAsync(response);
    }
}