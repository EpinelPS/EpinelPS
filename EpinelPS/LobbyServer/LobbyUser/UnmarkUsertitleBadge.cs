namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/lobby/usertitle/unmark-badge")]
public class UnmarkUserTitleBase : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUnMarkUserTitleBadge req = await ReadData<ReqUnMarkUserTitleBadge>();
        User user = GetUser();

        ResUnMarkUserTitleBadge response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
