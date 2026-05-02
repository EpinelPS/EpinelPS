using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/save")]
public class GetMvgSave : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var request = await ReadData<ReqSaveArcadeMvgData>();

        var user = GetUser();

        user.ArcadeInTheMirrorData = request.Data;

        await WriteDataAsync(new ResSaveArcadeMvgData());

        JsonDb.Save();
    }
}
