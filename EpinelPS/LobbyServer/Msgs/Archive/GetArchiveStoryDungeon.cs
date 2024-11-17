using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Msgs.Archive
{
    [PacketPath("/archive/storydungeon/get")]
    public class GetArchiveStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArchiveStoryDungeon>(); // has EventId field
            var evid = req.EventId;

            // Retrieve the user based on session (assuming GetCurrentUser is defined elsewhere)
            var user = GetUser();
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Access EventData directly
            if (!user.EventInfo.ContainsKey(evid))
            {
                throw new Exception($"Event with ID {evid} not found.");
            }

            var eventData = user.EventInfo[evid];

            // Prepare the response
            var response = new ResGetArchiveStoryDungeon();

            // Populate team data
            response.TeamData = new NetUserTeamData()
            {
                LastContentsTeamNumber = 1,
                Type = 1
            };

            // Populate the last cleared stage
            response.LastClearedArchiveStageList.Add(new NetLastClearedArchiveStage()
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });

            // Send the response
            await WriteDataAsync(response);
        }
    }
}
