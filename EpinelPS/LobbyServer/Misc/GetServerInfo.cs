namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/getserverinfo")]
public class GetServerInfo : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ResGetServerInfo r = new()
        {
            // todo: reimplement this as well
            MatchUrl = "https://global-match.nikke-kr.com",
            WorldId = 84
        };

        await WriteDataAsync(r);
    }
}
