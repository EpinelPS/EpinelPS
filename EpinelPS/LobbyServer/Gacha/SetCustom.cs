namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/Gacha/SetCustom")]
public class SetCustom : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetGachaCustom>();
        await WriteDataAsync(new ResSetGachaCustom());
    }
}
