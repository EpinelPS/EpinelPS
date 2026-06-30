using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Tutorial/Finish")]
public class TutorialFinish : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishMiniGameTtsTutorial req = await ReadData<ReqFinishMiniGameTtsTutorial>();
        User user = GetUser();
        ResFinishMiniGameTtsTutorial response = new();

        //Logging.WriteLine($"{req.EventTtsManagerTableId}", LogType.Info);

        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            ttsData.IsFinishTutorial = true;
        }

        JsonDb.Save();

        
        // TODO
        await WriteDataAsync(response);
    }
}