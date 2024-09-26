using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Minigame.nksv2
{
    [PacketPath("/minigame/nksv2/scenario/complete")]
    public class CompleteScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCompleteNKSV2Scenario>();
            var user = GetUser();

            var response = new ResCompleteNKSV2Scenario();
            user.MogInfo.CompletedScenarios.Add(req.ScenarioId);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
