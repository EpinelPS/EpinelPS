using EpinelPS.Utils;
using EpinelPS.Database;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/fastclearstage")]
    public class FastClearArchiveStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearArchiveStage req = await ReadData<ReqFastClearArchiveStage>();
            int evid = req.EventId;
            int stgid = req.StageId;

            User user = GetUser() ?? throw new Exception("User not found.");

            // Check if the EventInfo exists for the given EventId
            if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
            {
                throw new Exception($"Event with ID {evid} not found.");
            }
            // Update the LastStage in EventData
            eventData.LastStage = stgid;

			JsonDb.Save();
            ResFastClearArchiveStage response = new();

            // Send the response back to the client
            await WriteDataAsync(response);
        }
    }
}
