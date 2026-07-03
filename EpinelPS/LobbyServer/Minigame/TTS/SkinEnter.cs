using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Skin/Enter")]
public class SkinEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterMiniGameTtsSkin req = await ReadData<ReqEnterMiniGameTtsSkin>();
        User user = GetUser();
        ResEnterMiniGameTtsSkin response = new();
        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            response.EventTtsSkinObjectIdList.AddRange(ttsData.BuySkinObject);
        }
            // TODO
            
        await WriteDataAsync(response);
    }
}