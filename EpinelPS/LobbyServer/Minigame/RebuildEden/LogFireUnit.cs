namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/fireunit")]
public class LogFireUnit : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogFireArcadeRebuildEdenUnit req = await ReadData<ReqLogFireArcadeRebuildEdenUnit>();
        User user = GetUser();
        ResLogFireArcadeRebuildEdenUnit response = new();

        // TODO
        await WriteDataAsync(response);
    }
}