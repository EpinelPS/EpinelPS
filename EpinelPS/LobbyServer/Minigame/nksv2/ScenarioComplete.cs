using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/scenario/complete")]
public class ScenarioComplete : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteNKSV2Scenario req = await ReadData<ReqCompleteNKSV2Scenario>();
        User user = GetUser();
        ResCompleteNKSV2Scenario response = new();

        //Logging.WriteLine($"{req.DialogType},{req.NKsId},{req.ScenarioId}", LogType.Info);

        if (user.Nksv2Datas.TryGetValue(req.NKsId, out var nksv2Data))
        {
            nksv2Data.CompletedScenarios.AddUnique(req.ScenarioId);            
        }

        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}