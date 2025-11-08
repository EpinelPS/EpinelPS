using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/scenario/exist")]
    public class GetUserScenarioExist : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqExistScenario req = await ReadData<ReqExistScenario>();

            // TODO: Check response from real server

            ResExistScenario response = new();

            foreach (string? item in req.ScenarioGroupIds)
            {
                if (FindScenarioInMainStages(item) || FindScenarioInArchiveStages(item))
                {
                    response.ExistGroupIds.Add(item);
                }
            }

            await WriteDataAsync(response);
        }

        private bool FindScenarioInMainStages(string scenarioGroupId)
        {
            User user = GetUser();
            return user.CompletedScenarios.Contains(scenarioGroupId);
        }

        private bool FindScenarioInArchiveStages(string scenarioGroupId)
        {
            User user = GetUser();
            foreach (EventData evtData in user.EventInfo.Values)
            {
                if (evtData.CompletedScenarios.Contains(scenarioGroupId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
