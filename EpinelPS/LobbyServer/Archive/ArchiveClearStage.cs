using EpinelPS.Utils;
using EpinelPS.Database;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/clearstage")]
    public class ClearArchiveStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearArchiveStage req = await ReadData<ReqClearArchiveStage>(); // has fields EventId, StageId, BattleResult
            int evid = req.EventId;
            int stgid = req.StageId;
            int result = req.BattleResult;
            User user = GetUser();

            // Check if the EventInfo exists for the given EventId
            if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
            {
                throw new Exception($"Event with ID {evid} not found.");
            }

            // Update the EventData if BattleResult is 1
            if (result == 1 && !eventData.ClearedStages.Contains(stgid))
            {
                eventData.ClearedStages.Add(stgid);
                // Update the LastStage in EventData
                eventData.LastStage = stgid;
            }
			JsonDb.Save();
            ResClearArchiveStage response = new();

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
