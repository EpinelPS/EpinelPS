using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/scenario/complete")]
    public class CompleteEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetEventScenarioComplete>();
            var user = GetUser();

            if (user.EventInfo.ContainsKey(req.EventId))
            {
                var evt = user.EventInfo[req.EventId];
                evt.CompletedScenarios.Add(req.ScenarioId);
            }
            else
            {
                var evt = new EventData();
                evt.CompletedScenarios.Add(req.ScenarioId);
                user.EventInfo.Add(req.EventId, evt);
            }

            var response = new ResSetEventScenarioComplete();

            // TODO reward

            await WriteDataAsync(response);
        }
    }
}
