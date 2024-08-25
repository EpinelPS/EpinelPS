using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/user/scenario/exist")]
    public class GetUserScenarioExist : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExistScenario>();

            // TODO: Check response from real server

            var response = new ResExistScenario();

            var user = GetUser();

            foreach (var item in req.ScenarioGroupIds)
            {
                Console.WriteLine("check scenario " + item);
                foreach (var completed in user.CompletedScenarios)
                {
                    // story thingy was completed
                    if (completed == item)
                    {
                        Console.WriteLine(item + " is completed");
                        response.ExistGroupIds.Add(item);
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}
