namespace EpinelPS.LobbyServer.Auth;

[GameRequest("/auth/logout")]
public class AuthLogout : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLogout req = await ReadData<ReqLogout>();

        // TODO remove UsedAuthToken

        await WriteDataAsync(new ResLogout());
    }
}
