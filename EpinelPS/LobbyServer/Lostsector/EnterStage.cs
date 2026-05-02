namespace EpinelPS.LobbyServer.Lostsector;

[GameRequest("/lostsector/enterstage")]
public class EnterStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterLostSectorStage req = await ReadData<ReqEnterLostSectorStage>();

        ResEnterLostSectorStage response = new();

        await WriteDataAsync(response);
    }
}
