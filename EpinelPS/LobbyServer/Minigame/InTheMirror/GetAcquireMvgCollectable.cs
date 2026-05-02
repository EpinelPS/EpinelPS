using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/collectable")]
public class GetAcquireMvgCollectable : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var request = await ReadData<ReqAcquireArcadeMvgCollectable>();

        var user = GetUser();

        user.ArcadeInTheMirrorData.Collectables.Add(request.CollectableId);

        await WriteDataAsync(new ResAcquireArcadeMvgCollectable());

        JsonDb.Save();
    }
}