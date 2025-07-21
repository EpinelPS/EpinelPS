using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2
{
    [PacketPath("/minigame/nksv2/scenario/complete")]
    public class CompleteScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCompleteNKSV2Scenario req = await ReadData<ReqCompleteNKSV2Scenario>();
            User user = GetUser();

            ResCompleteNKSV2Scenario response = new();
            user.MogInfo.CompletedScenarios.Add(req.ScenarioId);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
