using EpinelPS.Utils;
using EpinelPS.Data;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/getresettable")]
    public class GetResettable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetResettableArchiveScenario req = await ReadData<ReqGetResettableArchiveScenario>();
            ResGetResettableArchiveScenario response = new(); // has ScenarioIdList field that takes in strings

            GameData gameData = GameData.Instance;
            User user = GetUser();
            foreach (ArchiveEventStoryRecord record in gameData.archiveEventStoryRecords.Values)
            {
                // Add the PrologueScenario to the ScenarioIdList
                if (record.EventId == req.EventId && !string.IsNullOrEmpty(record.PrologueScenario))
                {
                    if (user.EventInfo.TryGetValue(req.EventId, out EventData? evtData) &&
                        evtData.CompletedScenarios.Contains(record.PrologueScenario))
                    {
                        response.ScenarioIdList.Add(record.PrologueScenario);
                    }
                    break;
                }
            }

            await WriteDataAsync(response);
        }
    }
}
