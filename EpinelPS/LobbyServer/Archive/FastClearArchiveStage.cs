using EpinelPS.Utils;
using EpinelPS.Database;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/fastclearstage")]
    public class FastClearArchiveStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearArchiveStage>(); 
            var evid = req.EventId;
            var stgid = req.StageId;

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

            var eventData = user.EventInfo[evid];
            // Update the LastStage in EventData
            eventData.LastStage = stgid;

			JsonDb.Save();
            var response = new ResFastClearArchiveStage();

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
