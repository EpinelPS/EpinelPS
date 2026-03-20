using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.ScenarioChoice;

[PacketPath("/event/minigame/scenariochoice/update")]
public class Update : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqUpdateScenarioChoice req = await ReadData<ReqUpdateScenarioChoice>();
        User user = GetUser();

        ResUpdateScenarioChoice response = new();
        response.Data = ScenarioChoiceMain.Update(User, req.ScenarioChoiceManagerTableId, req.ScenarioScene, req.ChosenChoices);

        await WriteDataAsync(response);
    }
}