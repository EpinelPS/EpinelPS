using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/scenario/exist")]
    public class CheckScenarioExists : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqExistEventScenario req = await ReadData<ReqExistEventScenario>();

            ResExistEventScenario response = new();

            foreach (string? item in req.ScenarioGroupIds)
            response.ExistGroupIds.Add(item);

  
            await WriteDataAsync(response);
        }
    }
}
