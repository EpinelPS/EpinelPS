namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/minigame/nksv2/scenario/get")]
public class GetScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetNKSV2Scenario req = await ReadData<ReqGetNKSV2Scenario>();
        User user = GetUser();

        ResGetNKSV2Scenario response = new();
        response.ScenarioIdList.Add(user.MogInfo.CompletedScenarios);

        await WriteDataAsync(response);
    }
}
