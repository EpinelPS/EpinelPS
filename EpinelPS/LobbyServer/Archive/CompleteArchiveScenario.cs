using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.StaticInfo; // Ensure this namespace is included

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/complete")]
    public class CompleteScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCompleteArchiveScenario>(); // req has EventId, ScenarioId, DialogType fields
            var evid = req.EventId;
            var scenid = req.ScenarioId;
            var dialtyp = req.DialogType;

            var user = GetUser();

            // Ensure we are working with the user's EventInfo and not CompletedScenarios
            if (!user.EventInfo.TryGetValue(evid, out var evt))
            {
                // Create a new EventData if the event doesn't exist
                evt = new EventData();
                user.EventInfo[evid] = evt;
            }

            // Ensure the CompletedScenarios list is initialized and add the ScenarioId
            if (!evt.CompletedScenarios.Contains(scenid))
            {
                evt.CompletedScenarios.Add(scenid);
            }
			JsonDb.Save();
            // Prepare and send the response
            var response = new ResCompleteArchiveScenario();
            await WriteDataAsync(response);
        }
    }
}
