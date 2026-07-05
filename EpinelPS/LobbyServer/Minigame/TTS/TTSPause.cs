using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Pause")]
public class TTSPause : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqPauseMiniGameTtsPlay req = await ReadData<ReqPauseMiniGameTtsPlay>();
        //GameUser user = GetUser();
        ResPauseMiniGameTtsPlay response = new();
        
        // TODO
        await WriteDataAsync(response);
    }
}