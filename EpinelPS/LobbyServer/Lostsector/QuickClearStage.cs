using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Lostsector;

[GameRequest("/lostsector/fastclearstage")]
public class QuickClearStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearLostSectorStage req = await ReadData<ReqFastClearLostSectorStage>();
        User user = GetUser();

        ResFastClearLostSectorStage response = new();

        ClearStage.ClearLostSectorStage(user, req.StageId);
        JsonDb.Save();

        await WriteDataAsync(response);
    }

}
