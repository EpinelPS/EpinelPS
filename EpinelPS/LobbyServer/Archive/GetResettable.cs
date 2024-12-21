using EpinelPS.Utils;
using EpinelPS.StaticInfo;
namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/scenario/getresettable")]
    public class GetResettable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetResettableArchiveScenario>(); 
            var response = new ResGetResettableArchiveScenario(); // has ScenarioIdList field that takes in strings
            
            // Retrieve stage IDs from GameData
            var stageIds = GameData.Instance.archiveEventDungeonStageRecords.Values.Select(record => record.stage_id.ToString()).ToList();

            // Add them to the response
            response.ScenarioIdList.Add(stageIds);

            await WriteDataAsync(response);
        }
    }
}
