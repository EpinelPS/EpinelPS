namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/dispatch")]
public class LogDispatch : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogArcadeRebuildEdenDispatch req = await ReadData<ReqLogArcadeRebuildEdenDispatch>();
        User user = GetUser();
        ResLogArcadeRebuildEdenDispatch response = new();

        await WriteDataAsync(response);
    }
}