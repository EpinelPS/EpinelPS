using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setprofileicon")]
public class SetProfileIcon : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetProfileIcon req = await ReadData<ReqSetProfileIcon>();
        User user = GetUser();
        user.ProfileIconId = req.Icon;
        user.ProfileIconIsPrism = req.IsPrism;
        JsonDb.Save();
        ResSetProfileIcon response = new();

        await WriteDataAsync(response);
    }
}
