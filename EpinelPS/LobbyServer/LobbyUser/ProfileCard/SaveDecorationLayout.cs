using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser.ProfileCard;

[GameRequest("/ProfileCard/DecorationLayout/Save")]
public class SaveDecorationLayout : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqSaveProfileCardDecorationLayout>();
        var user = GetUser();
        user.ProfileCardDecoration = req.Layout;
        var res = new ResSaveProfileCardDecorationLayout();
        JsonDb.Save();
        await WriteDataAsync(res);
    }
}
