using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.ScenarioChoice;

[PacketPath("/event/minigame/scenariochoice/get")]
public class GetData : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqGetScenarioChoiceData req = await ReadData<ReqGetScenarioChoiceData>();

        ResGetScenarioChoiceData response = new()
        {
            Data = ScenarioChoiceMain.GetData(User, req.ScenarioChoiceManagerTableId)
        };

        await WriteDataAsync(response);
    }
}
