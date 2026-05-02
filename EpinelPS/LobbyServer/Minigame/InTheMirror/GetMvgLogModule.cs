namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/module")]
public class GetMvgLogModule : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqEquipArcadeMvgModule>();

        await WriteDataAsync(new ResEquipArcadeMvgModule());
    }
}
