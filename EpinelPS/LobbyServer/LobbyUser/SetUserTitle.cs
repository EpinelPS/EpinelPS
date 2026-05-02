using EpinelPS.Database;
namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/lobby/usertitle/set")]
public class SetUserTitleData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetUserTitle req = await ReadData<ReqSetUserTitle>();
        User user = GetUser();
        user.TitleId = req.UserTitleId;
        JsonDb.Save();
        ResSetUserTitle response = new();

        await WriteDataAsync(response);
    }
}
