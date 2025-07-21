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

            Database.User user = GetUser();

            foreach (string? item in req.ScenarioGroupIds)
            {
                foreach (string completed in user.CompletedScenarios)
                {
                    // story thingy was completed
                    if (completed == item)
                    {
                        response.ExistGroupIds.Add(item);
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
