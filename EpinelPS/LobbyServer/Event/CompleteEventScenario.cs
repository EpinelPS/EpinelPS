using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/scenario/complete")]
public class CompleteEventScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetEventScenarioComplete req = await ReadData<ReqSetEventScenarioComplete>();
        User user = GetUser();

        if (user.EventInfo.TryGetValue(req.EventId, out EventData? evt))
        {
            if (!evt.CompletedScenarios.Contains(req.ScenarioId))
            {
                evt.CompletedScenarios.Add(req.ScenarioId);
            }
        }
        else
        {
            evt = new();
            evt.CompletedScenarios.Add(req.ScenarioId);
            user.EventInfo.Add(req.EventId, evt);
        }

        JsonDb.Save();
        ResSetEventScenarioComplete response = new();

        // TODO reward

        await WriteDataAsync(response);
    }
}
