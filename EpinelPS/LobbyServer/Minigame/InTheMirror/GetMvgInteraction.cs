namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/interaction")]
public class GetMvgLogInteraction : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqLogArcadeMvgInteraction>();

        await WriteDataAsync(new ResLogArcadeMvgInteraction());

    }
}
