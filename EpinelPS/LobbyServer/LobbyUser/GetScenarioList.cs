using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetScenarioList")]
    public class GetScenarioList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetScenarioList>();
            var user = GetUser();

            // todo what are bookmark scenarios?

            // this returns a list of scenarios that user has completed
            var response = new ResGetScenarioList();
            foreach (var item in user.CompletedScenarios)
            {
                response.ScenarioList.Add(item);
            }

            await WriteDataAsync(response);
        }
    }
}
