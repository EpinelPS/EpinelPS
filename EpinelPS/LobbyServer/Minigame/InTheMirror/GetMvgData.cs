namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/get")]
public class GetMvgData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetArcadeMvgData>();

        var user = GetUser();

        await WriteDataAsync(new ResGetArcadeMvgData() { Data = user.ArcadeInTheMirrorData });

    }
}
