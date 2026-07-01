using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Start")]
public class TTSStart : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqStartMiniGameTtsPlay req = await ReadData<ReqStartMiniGameTtsPlay>();
        User user = GetUser();
        ResStartMiniGameTtsPlay response = new();
        await WriteDataAsync(response);
    }
}