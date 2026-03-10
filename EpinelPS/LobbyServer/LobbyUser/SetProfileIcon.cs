using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setprofileicon")]
    public class SetProfileIcon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetProfileIcon req = await ReadData<ReqSetProfileIcon>();
            User user = User;
            user.ProfileIconId = req.Icon;
            user.ProfileIconIsPrism = req.IsPrism;
            JsonDb.Save();
            ResSetProfileIcon response = new();

            await WriteDataAsync(response);
        }
    }
}
