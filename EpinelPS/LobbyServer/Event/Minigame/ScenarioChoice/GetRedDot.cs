using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.ScenarioChoiceis;

[PacketPath("/event/minigame/scenariochoice/reddot")]
public class GetRedDot : LobbyMsgHandler
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