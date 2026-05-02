namespace EpinelPS.LobbyServer.Event.Minigame.ScenarioChoiceis;

[GameRequest("/event/minigame/scenariochoice/reddot")]
public class GetRedDot : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExistScenarioChoiceRedDotData req = await ReadData<ReqExistScenarioChoiceRedDotData>();
        User user = GetUser();

        ResExistScenarioChoiceRedDotData response = new()
        {

        };

        // TODO implement properly

        await WriteDataAsync(response);
    }
}