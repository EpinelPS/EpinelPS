using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setprofileicon")]
    public class SetProfileIcon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetProfileIcon>();
            var user = GetUser();
            user.ProfileIconId = req.Icon;
            user.ProfileIconIsPrism = req.IsPrism;
            JsonDb.Save();
            var response = new ResSetProfileIcon();

            await WriteDataAsync(response);
        }
    }
}
