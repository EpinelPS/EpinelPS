using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/scenario/get")]
    public class GetEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventScenarioData>();
            var user = GetUser();

            var response = new ResGetEventScenarioData();
            if (user.EventInfo.ContainsKey(req.EventID))
            {
                var evt = user.EventInfo[req.EventID];
                response.ScenarioIdList.AddRange(evt.CompletedScenarios);
            }
            else
            {
                user.EventInfo.Add(req.EventID, new EventData());
            }

            await WriteDataAsync(response);
        }
    }
}
