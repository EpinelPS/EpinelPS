using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/AlbumRedDotCutoffDate/Update")]
public class AlbumRedDotCutoffDateUpdate : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUpdateTtsAlbumRedDotCutoffDate req = await ReadData<ReqUpdateTtsAlbumRedDotCutoffDate>();
        User user = GetUser();
        ResUpdateTtsAlbumRedDotCutoffDate response = new();

        //Logging.WriteLine($"{req.Timestamp},{req.Type}", LogType.Info);

        switch (req.Type)
        {
            case TtsAlbumRedDotCutoffDateType.Shop:
                user.TTSGameData[1].DateFromShop = req.Timestamp;
                break;
            case TtsAlbumRedDotCutoffDateType.NewProductPopUp:
                user.TTSGameData[1].NewProductPopUp = req.Timestamp;
                break;
            default:
                break;
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}