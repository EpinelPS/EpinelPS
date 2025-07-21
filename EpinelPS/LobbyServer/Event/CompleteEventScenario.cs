using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/scenario/complete")]
    public class CompleteEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetEventScenarioComplete req = await ReadData<ReqSetEventScenarioComplete>();
            User user = GetUser();

            if (user.EventInfo.TryGetValue(req.EventId, out EventData? evt))
            {
                evt.CompletedScenarios.Add(req.ScenarioId);
            }
            else
            {
                evt = new();
                evt.CompletedScenarios.Add(req.ScenarioId);
                user.EventInfo.Add(req.EventId, evt);
            }

            ResSetEventScenarioComplete response = new();

            // TODO reward

            await WriteDataAsync(response);
        }
    }
}
