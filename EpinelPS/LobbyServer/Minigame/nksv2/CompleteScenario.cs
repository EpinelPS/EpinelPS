using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/minigame/nksv2/scenario/complete")]
public class CompleteScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteNKSV2Scenario req = await ReadData<ReqCompleteNKSV2Scenario>();
        User user = GetUser();

        ResCompleteNKSV2Scenario response = new();
        user.MogInfo.CompletedScenarios.Add(req.ScenarioId);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
