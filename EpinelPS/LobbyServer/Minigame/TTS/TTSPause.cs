using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Pause")]
public class TTSPause : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqPauseMiniGameTtsPlay req = await ReadData<ReqPauseMiniGameTtsPlay>();
        //User user = GetUser();
        ResPauseMiniGameTtsPlay response = new();

        //Logging.WriteLine($"{req.EventTtsManagerTableId}", LogType.Info);
        
        // TODO NO
        await WriteDataAsync(response);
    }
}