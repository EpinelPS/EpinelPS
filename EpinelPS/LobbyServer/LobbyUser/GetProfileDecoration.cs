namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/ProfileCard/DecorationLayout/Get")]
public class GetProfileDecoration : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqProfileCardDecorationLayout req = await ReadData<ReqProfileCardDecorationLayout>();

        ResProfileCardDecorationLayout r = new()
        {
            Layout = new ProfileCardDecorationLayout
            {
                BackgroundId = 101002,
                ShowCharacterSpine = true
            }
        };
        await WriteDataAsync(r);
    }
}
