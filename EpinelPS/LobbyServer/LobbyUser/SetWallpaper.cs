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
            user.WallpaperList = [.. req.WallpaperList];
            user.WallpaperBackground = [.. req.WallpaperBackgroundList];
            user.WallpaperFavoriteList = [.. req.WallpaperFavoriteList];
            user.WallpaperPlaylistList = [.. req.WallpaperPlaylistList];
            user.WallpaperJukeboxList = [.. req.WallpaperJukeboxList];

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
