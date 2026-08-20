namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/ProfileCard/DecorationLayout/Get")]
public class GetProfileDecoration : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqProfileCardDecorationLayout req = await ReadData<ReqProfileCardDecorationLayout>();
        var user = GetUser();
        ResProfileCardDecorationLayout res = new();
        res.Layout = user.ProfileCardDecoration is not null && user.ProfileCardDecoration.BackgroundId != 0
            ? user.ProfileCardDecoration
            : res.Layout = new ProfileCardDecorationLayout
            {
                BackgroundId = 301001,
                ShowCharacterSpine = true
            };
        await WriteDataAsync(res);
    }
}
