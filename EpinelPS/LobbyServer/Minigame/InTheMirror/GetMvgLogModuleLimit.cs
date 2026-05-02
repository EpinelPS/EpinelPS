namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/module/limit")]
public class GetMvgLogModuleLimit : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqIncreaseArcadeMvgModuleLimit>();

        await WriteDataAsync(new ResIncreaseArcadeMvgModuleLimit());
    }
}
