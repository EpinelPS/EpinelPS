using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setprofiledata")]
public class SetProfileData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetProfileData req = await ReadData<ReqSetProfileData>();
        User user = GetUser();
        user.ProfileIconId = req.Icon;
        user.ProfileIconIsPrism = req.IsPrism;
        user.ProfileFrame = req.Frame;

        JsonDb.Save();
        ResSetProfileData response = new();

        await WriteDataAsync(response);
    }
}
