using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Minigame.nksv2
{
    [PacketPath("/minigame/nksv2/scenario/get")]
    public class GetScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetNKSV2Scenario>();
            var user = GetUser();

            var response = new ResGetNKSV2Scenario();
            response.ScenarioIdList.Add(user.MogInfo.CompletedScenarios);

            await WriteDataAsync(response);
        }
    }
}
