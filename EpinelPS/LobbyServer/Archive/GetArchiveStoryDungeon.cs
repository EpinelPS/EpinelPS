using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/get")]
    public class GetArchiveStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetArchiveStoryDungeon req = await ReadData<ReqGetArchiveStoryDungeon>(); // has EventId field
            int evid = req.EventId;
            User user = GetUser();

            // Ensure the EventInfo dictionary contains the requested EventId
            if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
            {
                eventData = new EventData
                {
                    CompletedScenarios = [], // Initialize empty list
                    Diff = 0,                                // Default difficulty
                    LastStage = 0                            // Default last cleared stage
                };
                // Create a new default entry for the missing EventId
                user.EventInfo[evid] = eventData;
				JsonDb.Save();
            }

            // Prepare the response
            ResGetArchiveStoryDungeon response = new()
            {
                // Populate team data
                TeamData = new NetUserTeamData
                {
                    LastContentsTeamNumber = 1,
                    Type = 1
                }
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
