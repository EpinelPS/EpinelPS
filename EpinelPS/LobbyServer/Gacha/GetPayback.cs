namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/getpayback")]
public class GetPayback : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetGachaPaybackData req = await ReadData<ReqGetGachaPaybackData>();

        ResGetGachaPaybackData response = new();

        // TODO implement

        await WriteDataAsync(response);
    }
}
