using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Resume")]
public class TTSResume : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResumeMiniGameTtsPlay req = await ReadData<ReqResumeMiniGameTtsPlay>();
        User user = GetUser();
        ResResumeMiniGameTtsPlay response = new();

        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            ttsData.TotalPlayTime = TtsHelper.Subtract(ttsData.TotalPlayTime, req.PauseDuration);
        }
        await WriteDataAsync(response);
    }
}
