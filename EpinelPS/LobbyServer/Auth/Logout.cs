namespace EpinelPS.LobbyServer.Auth;

[GameRequest("/auth/logout")]
[GameRequest("/logout")]
public class LogoutHandler : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqLogout>();
        ResLogout response = new();
        await WriteDataAsync(response);
    }
}
