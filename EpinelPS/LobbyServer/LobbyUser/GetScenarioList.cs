using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetScenarioList")]
    public class GetScenarioList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetScenarioList req = await ReadData<ReqGetScenarioList>();
            Database.User user = GetUser();

            // todo what are bookmark scenarios?

            // this returns a list of scenarios that user has completed
            ResGetScenarioList response = new();
            foreach (string item in user.CompletedScenarios)
            {
                response.ScenarioList.Add(item);
            }

            await WriteDataAsync(response);
        }
    }
}
