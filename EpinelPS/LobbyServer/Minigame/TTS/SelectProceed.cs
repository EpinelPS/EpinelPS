using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Expert/PopUp/Select/Proceed")]
public class SelectProceed : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSelectProceedOnMiniGameTtsExpertAlertPopUp req = await ReadData<ReqSelectProceedOnMiniGameTtsExpertAlertPopUp>();
        User user = GetUser();
        ResSelectProceedOnMiniGameTtsExpertAlertPopUp response = new();

        //Logging.WriteLine($"{req.EventTtsManagerTableId}", LogType.Info);

        // TODO
        
        await WriteDataAsync(response);
    }
}