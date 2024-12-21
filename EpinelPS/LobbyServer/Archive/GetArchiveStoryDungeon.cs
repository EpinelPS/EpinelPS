using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/get")]
    public class GetArchiveStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArchiveStoryDungeon>(); // has EventId field
            var evid = req.EventId;
            var user = GetUser();

            // Ensure the EventInfo dictionary contains the requested EventId
            if (!user.EventInfo.ContainsKey(evid))
            {
                // Create a new default entry for the missing EventId
                user.EventInfo[evid] = new EventData
                {
                    CompletedScenarios = new List<string>(), // Initialize empty list
                    Diff = 0,                                // Default difficulty
                    LastStage = 0                            // Default last cleared stage
                };
				JsonDb.Save();
            }

            var eventData = user.EventInfo[evid];

            // Prepare the response
            var response = new ResGetArchiveStoryDungeon();

            // Populate team data
            response.TeamData = new NetUserTeamData
            {
                LastContentsTeamNumber = 1,
                Type = 1
            };

            // Populate the last cleared stage
            response.LastClearedArchiveStageList.Add(new NetLastClearedArchiveStage
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });

            // Send the response
            await WriteDataAsync(response);
        }
    }
}
