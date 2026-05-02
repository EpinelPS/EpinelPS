namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/unlock")]
public class GetMvgLogUpdate : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqUnlockArcadeMvg>();

        await WriteDataAsync(new ResUnlockArcadeMvg());
    }
}
