namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/state")]
public class GetMvgLogState : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqLogArcadeMvgState>();

        await WriteDataAsync(new ResLogArcadeMvgState());
    }
}
