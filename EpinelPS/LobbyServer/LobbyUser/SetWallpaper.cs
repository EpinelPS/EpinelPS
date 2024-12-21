using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
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
            user.WallpaperBackground = req.WallpaperBackgroundList.ToArray();
            user.WallpaperFavoriteList = req.WallpaperFavoriteList.ToArray();
            user.WallpaperPlaylistList = req.WallpaperPlaylistList.ToArray();
            user.WallpaperJukeboxList = req.WallpaperJukeboxList.ToArray();

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
