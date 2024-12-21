using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.StaticInfo; // For GameData access
using System.Linq;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/scenario/get")]
    public class GetEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventScenarioData>(); 

            // Access GameData instance
            var gameData = GameData.Instance;

            // Create a response object
            var response = new ResGetEventScenarioData();

            // Get all scenario_group_id values from albumResourceRecords starting with "event_"
            response.ScenarioIdList.Add(gameData.albumResourceRecords.Values.Where(record => record.scenario_group_id.StartsWith("event_")).Select(record => record.scenario_group_id).ToList());

            await WriteDataAsync(response);
        }
    }
}
