namespace EpinelPS.LobbyServer.Minigame.PlaySoda;

[GameRequest("/arcade/enterlog")]
public class GetEnterArcadeLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var request = await ReadData<ReqEnterArcadeLog>();

        await WriteDataAsync(new ResEnterArcadeLog());
    }
}
