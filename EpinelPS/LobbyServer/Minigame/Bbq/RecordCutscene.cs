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

        NetArcadeBBQData? bBQData = user.BBQInfoData;
        List<int>? list = bBQData.RecordedCutSceneList.ToList();

        user.AddUnique(list, req.CutSceneTid);

        bBQData.RecordedCutSceneList.Clear();    
        bBQData.RecordedCutSceneList.AddRange(list);

        response.Data = bBQData;

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}