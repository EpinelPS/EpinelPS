namespace EpinelPS.LobbyServer.Event.Itabag;

[GameRequest("/Event/Itabag/RedDotData")]
public class GetRedDot : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqItabagRedDotData req = await ReadData<ReqItabagRedDotData>();
        User user = GetUser();

        ReqItabagRedDotData res = new();

        // TODO
        await WriteDataAsync(res);
    }
}
