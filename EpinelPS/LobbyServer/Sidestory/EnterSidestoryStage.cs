namespace EpinelPS.LobbyServer.Sidestory;

[GameRequest("/sidestory/stage/enter")]
public class EnterSidestoryStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterSideStoryStage req = await ReadData<ReqEnterSideStoryStage>();
        User user = GetUser();

        ResEnterSideStoryStage response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
