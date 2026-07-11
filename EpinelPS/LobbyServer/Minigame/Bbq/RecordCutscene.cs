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
        
        user.BBQInfoData.RecordedCutSceneList.AddUnique(req.CutSceneTid);
        response.Data = MiniGameHelper.BBQToNet(user.BBQInfoData);

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}