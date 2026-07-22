namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/log/completequest")]
public class LogCompleteQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogCompleteArcadeRebuildEdenQuest req = await ReadData<ReqLogCompleteArcadeRebuildEdenQuest>();
        User user = GetUser();
        ResLogCompleteArcadeRebuildEdenQuest response = new();

        await WriteDataAsync(response);
    }
}