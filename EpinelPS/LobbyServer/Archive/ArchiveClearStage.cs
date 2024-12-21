using EpinelPS.Utils;
using EpinelPS.Database;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/clearstage")]
    public class ClearArchiveStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearArchiveStage>(); // has fields EventId, StageId, BattleResult
            var evid = req.EventId;
            var stgid = req.StageId;
            var result = req.BattleResult; // if 2 add to event info as last stage
            var user = GetUser();

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Check if the EventInfo exists for the given EventId
            if (!user.EventInfo.ContainsKey(evid))
            {
                throw new Exception($"Event with ID {evid} not found.");
            }

            // Update the EventData if BattleResult is 2
            if (result == 1)
            {
                var eventData = user.EventInfo[evid];

                // Update the LastStage in EventData
                eventData.LastStage = stgid;

            }
			JsonDb.Save();
            var response = new ResClearArchiveStage();

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
