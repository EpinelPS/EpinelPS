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

            // Retrieve stage IDs from GameData
            List<string> stageIds = [.. GameData.Instance.archiveEventDungeonStageRecords.Values.Select(record => record.StageId.ToString())];

            // Add them to the response
            response.ScenarioIdList.Add(stageIds);

            await WriteDataAsync(response);
        }
    }
}
