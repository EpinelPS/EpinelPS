using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/scenario/exist")]
    public class CheckScenarioExists : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExistEventScenario>();

            var response = new ResExistEventScenario();

            foreach (var item in req.ScenarioGroupIds)
            response.ExistGroupIds.Add(item);

  
            await WriteDataAsync(response);
        }
    }
}
