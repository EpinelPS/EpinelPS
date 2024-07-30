using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/User/SetWallpaper")]
    public class SetWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetWallpaper>();
            var response = new ResSetWallpaper();
            var user = GetUser();
            user.WallpaperList = req.WallpaperList.ToArray();

            await WriteDataAsync(response);
        }
    }
}
