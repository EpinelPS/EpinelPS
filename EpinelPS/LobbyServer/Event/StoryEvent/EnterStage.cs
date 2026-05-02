namespace EpinelPS.LobbyServer.Event.StoryEvent;

[GameRequest("/event/storydungeon/enterstage")]
public class EnterEventStoryStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterEventStage req = await ReadData<ReqEnterEventStage>();
        User user = GetUser();
        ResEnterEventStage response = new();

        await WriteDataAsync(response);
    }
}
