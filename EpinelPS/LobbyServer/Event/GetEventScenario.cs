using EpinelPS.Utils;
using EpinelPS.Data; // For GameData access
using System.Linq;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/scenario/get")]
    public class GetEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetEventScenarioData req = await ReadData<ReqGetEventScenarioData>();
            User user = GetUser();

            ResGetEventScenarioData response = new();

            /*
            if (user.EventInfo.TryGetValue(req.EventId, out EventData? data))
            {
                response.ScenarioIdList.AddRange(data.CompletedScenarios);
            }
            */

            // Get all scenario_group_id values from albumResourceRecords starting with "event_"
            response.ScenarioIdList.Add(GameData.Instance.albumResourceRecords.Values.Where(record => record.scenario_group_id.StartsWith("event_")).Select(record => record.scenario_group_id).ToList());

            await WriteDataAsync(response);
        }
    }
}
