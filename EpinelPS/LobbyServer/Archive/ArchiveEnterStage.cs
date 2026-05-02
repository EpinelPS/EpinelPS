namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/storydungeon/enterstage")]
public class EnterArchiveStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArchiveStage req = await ReadData<ReqEnterArchiveStage>();// has fields EventId StageId TeamNumber
        int evid = req.EventId;

        ResEnterArchiveStage response = new();

        await WriteDataAsync(response);
    }
}
