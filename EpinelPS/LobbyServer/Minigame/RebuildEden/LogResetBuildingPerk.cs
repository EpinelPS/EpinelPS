namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/resetbuildingperk")]
public class LogResetBuildingPerk : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogResetArcadeRebuildEdenBuildingPerk req = await ReadData<ReqLogResetArcadeRebuildEdenBuildingPerk>();
        User user = GetUser();
        ResLogResetArcadeRebuildEdenBuildingPerk response = new();

        // TODO
        await WriteDataAsync(response);
    }
}