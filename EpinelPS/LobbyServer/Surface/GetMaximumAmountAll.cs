namespace EpinelPS.LobbyServer.Surface;

[GameRequest("/Surface/Export/MaxAmount/All")]
public class GetMaximumAmountAll : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListAllSurfaceCurrencyMaxAmount req = await ReadData<ReqListAllSurfaceCurrencyMaxAmount>();
        User user = GetUser();

        ResListAllSurfaceCurrencyMaxAmount response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
