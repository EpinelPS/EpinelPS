namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/pity/list")]
public class ListPity : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListGachaPityProgress req = await ReadData<ReqListGachaPityProgress>();

        ResListGachaPityProgress response = new();

        // TODO implement

        await WriteDataAsync(response);
    }
}
