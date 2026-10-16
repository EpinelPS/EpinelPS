namespace EpinelPS.LobbyServer.EventQuest;

[GameRequest("/eventquest/stage/enter")]
[GameRequest("/eventquest/enterstage")]
[GameRequest("/event-quest/enterstage")]
[GameRequest("/event-quest/stage/enter")]
[GameRequest("/event/event-quest/enterstage")]
public class EnterEventQuestStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqEnterEventQuestStage>();
        ResEnterEventQuestStage response = new();
        await WriteDataAsync(response);
    }
}
