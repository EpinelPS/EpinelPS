using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Bbq;

[GameRequest("/arcade/bbq/recordcutscene")]
public class RecordCutscene : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordArcadeBBQCutScene req = await ReadData<ReqRecordArcadeBBQCutScene>();
        User user = GetUser();
        ResRecordArcadeBBQCutScene response = new();

        if (user.BBQInfoData.RecordedCutScenes.Contains(req.CutSceneTid))
            user.BBQInfoData.RecordedCutScenes.Add(req.CutSceneTid);

        response.Data = user.BBQInfoData.ToNet();

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}