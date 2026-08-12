namespace EpinelPS.LobbyServer.Event.Minigame.ScenarioChoice;

[GameRequest("/event/minigame/scenariochoice/get")]
public class GetData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetScenarioChoiceData req = await ReadData<ReqGetScenarioChoiceData>();
        var user = GetUser();

        ResGetScenarioChoiceData response = new()
        {
            Data = ScenarioChoiceMain.GetData(user, req.ScenarioChoiceManagerTableId)
        };

        await WriteDataAsync(response);
    }
}
