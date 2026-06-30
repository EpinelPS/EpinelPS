using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/PlayTime/Save")]
public class PlayTimeSave : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveMiniGameTtsPlayTime req = await ReadData<ReqSaveMiniGameTtsPlayTime>();
        User user = GetUser();
        ResSaveMiniGameTtsPlayTime response = new();

        //Logging.WriteLine($"{req.EventTtsManagerTableId},{req.TotalPlayTime}", LogType.Info);

        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            ttsData.TotalPlayTime = req.TotalPlayTime;
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}