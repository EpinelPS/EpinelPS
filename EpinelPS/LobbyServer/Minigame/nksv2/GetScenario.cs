using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2
{
    [PacketPath("/minigame/nksv2/scenario/get")]
    public class GetScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetNKSV2Scenario req = await ReadData<ReqGetNKSV2Scenario>();
            Database.User user = GetUser();

            ResGetNKSV2Scenario response = new();
            response.ScenarioIdList.Add(user.MogInfo.CompletedScenarios);

            await WriteDataAsync(response);
        }
    }
}
