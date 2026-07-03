using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame;

[GameRequest("/arcade/scenario/complete")]
public class ScenarioComplete : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteArcadeScenario req = await ReadData<ReqCompleteArcadeScenario>();
        User user = GetUser();
        ResCompleteArcadeScenario response = new();

        if (user.MiniGameScenarios.TryGetValue(req.ArcadeId, out MiniGameScenarios? evt))
        {
            if (!evt.CompletedScenarios.Contains(req.ScenarioId))
            {
                evt.CompletedScenarios.Add(req.ScenarioId);
            }
        }
        else
        {
            evt = new MiniGameScenarios();
            evt.CompletedScenarios.Add(req.ScenarioId);
            user.MiniGameScenarios.Add(req.ArcadeId, evt);
        }

        JsonDb.Save();
        
        // TODO
        await WriteDataAsync(response);
    }
}