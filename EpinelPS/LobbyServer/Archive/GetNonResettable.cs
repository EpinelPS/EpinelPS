using EpinelPS.Utils;
using EpinelPS.Data; // Ensure this namespace is included


namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/getnonresettable")]
    public class GetNonResettable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetNonResettableArchiveScenario req = await ReadData<ReqGetNonResettableArchiveScenario>(); // req has EventId field
            int evId = req.EventId;
            ResGetNonResettableArchiveScenario response = new();

            // Access the GameData instance
            GameData gameData = GameData.Instance;

            if (evId == 130002)
            {
                // Directly use the archiveEventQuestRecords dictionary
                foreach (ArchiveEventQuestRecord_Raw record in gameData.archiveEventQuestRecords.Values)
                {
                    if (record.EventQuestManagerId == evId)
                    {
                        // Add the end_scenario_Id to the ScenarioIdList
                        if (!string.IsNullOrEmpty(record.EndScenarioId))
                        {
                            response.ScenarioIdList.Add(record.EndScenarioId);
                        }
                    }
                }
            }
            else
            {
                // Directly use the archiveEventStoryRecords dictionary
                foreach (ArchiveEventStoryRecord record in gameData.archiveEventStoryRecords.Values)
                {
                    if (record.EventId == evId)
                    {
                        // Add the PrologueScenario to the ScenarioIdList
                        if (!string.IsNullOrEmpty(record.PrologueScenario))
                        {
                            response.ScenarioIdList.Add(record.PrologueScenario);
                        }
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
