using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/GetWallpaper")]
public class GetWallpaper : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetWallpaper req = await ReadData<ReqGetWallpaper>();
        ResGetWallpaper response = new();
        User user = GetUser();


        response.WallpaperList.AddRange(user.WallpaperList);
        response.WallpaperPlaylistList.AddRange(user.WallpaperPlaylistList);
        response.WallpaperJukeboxList.AddRange(user.WallpaperJukeboxList);
        response.WallpaperBackgroundList.AddRange(user.WallpaperBackground);
        response.WallpaperFavoriteList.AddRange(user.WallpaperFavoriteList);
        response.OwnedLobbyDecoBackgroundIdList.AddRange(user.LobbyDecoBackgroundList);

        response.JukeboxIdList.AddRange(JukeboxUtils.GetUnlockedSongs(user));

        await WriteDataAsync(response);
    }
}
