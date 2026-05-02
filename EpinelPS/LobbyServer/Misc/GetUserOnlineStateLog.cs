namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/useronlinestatelog")]
public class GetUserOnlineStateLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUserOnlineStateLog req = await ReadData<ReqUserOnlineStateLog>();
        User user = GetUser();

        ResUserOnlineStateLog response = new();
        user.LastLogin = DateTime.UtcNow;
        await WriteDataAsync(response);
    }
}
