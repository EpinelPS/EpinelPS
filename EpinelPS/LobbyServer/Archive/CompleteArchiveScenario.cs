using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/complete")]
    public class CompleteScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCompleteArchiveScenario req = await ReadData<ReqCompleteArchiveScenario>(); // req has EventId, ScenarioId, DialogType fields
            int evid = req.EventId;
            string scenid = req.ScenarioId;
            int dialtyp = req.DialogType;

            User user = GetUser();

            // Ensure we are working with the user's EventInfo and not CompletedScenarios
            if (!user.EventInfo.TryGetValue(evid, out EventData? evt))
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
            ResCompleteArchiveScenario response = new();
            await WriteDataAsync(response);
        }
    }
}
