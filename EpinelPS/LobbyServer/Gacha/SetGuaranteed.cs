namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/set/guaranteed")]
public class SetGuaranteed : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetGachaGuaranteed>();
        await WriteDataAsync(new ResSetGachaGuaranteed());
    }
}
