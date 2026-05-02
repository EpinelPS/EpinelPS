using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/SetWallpaper")]
public class SetWallpaper : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetWallpaper req = await ReadData<ReqSetWallpaper>();
        ResSetWallpaper response = new();
        User user = GetUser();
        user.WallpaperList = [.. req.WallpaperList];
        user.WallpaperBackground = [.. req.WallpaperBackgroundList];
        user.WallpaperFavoriteList = [.. req.WallpaperFavoriteList];
        user.WallpaperPlaylistList = [.. req.WallpaperPlaylistList];
        user.WallpaperJukeboxList = [.. req.WallpaperJukeboxList];

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
