using EpinelPS.Utils;
using EpinelPS.StaticInfo; // Ensure this namespace is included


namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/getnonresettable")]
    public class GetNonResettable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetNonResettableArchiveScenario>(); // req has EventId field
            var evid = req.EventId;
            var response = new ResGetNonResettableArchiveScenario();

            // Access the GameData instance
            var gameData = GameData.Instance;

            if (evid == 130002)
            {
                // Directly use the archiveEventQuestRecords dictionary
                foreach (var record in gameData.archiveEventQuestRecords.Values)
                {
                    if (record.event_quest_manager_id == evid)
                    {
                        // Add the end_scenario_id to the ScenarioIdList
                        if (!string.IsNullOrEmpty(record.end_scenario_id))
                        {
                            response.ScenarioIdList.Add(record.end_scenario_id);
                        }
                    }
                }
            }
            else
            {
                // Directly use the archiveEventStoryRecords dictionary
                foreach (var record in gameData.archiveEventStoryRecords.Values)
                {
                    if (record.event_id == evid)
                    {
                        // Add the prologue_scenario to the ScenarioIdList
                        if (!string.IsNullOrEmpty(record.prologue_scenario))
                        {
                            response.ScenarioIdList.Add(record.prologue_scenario);
                        }
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
