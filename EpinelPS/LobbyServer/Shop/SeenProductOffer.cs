namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/productoffer/setseen")]
public class SeenProductOffer : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetSetSeenProductOffer x = await ReadData<ReqSetSetSeenProductOffer>();

        // TODO: Figure out a way to disable ads

        ResSetSetSeenProductOffer response = new();

        await WriteDataAsync(response);
    }
}
