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
        response.WallpaperPlaylistList.AddRange(user.WallpaperPlaylistList.Where(wp => user.PlayLists.Any(p => p.JukeboxPlaylistUid == wp.PlaylistUId)));
        response.WallpaperJukeboxList.AddRange(user.WallpaperJukeboxList);
        response.WallpaperBackgroundList.AddRange(user.WallpaperBackground);
        response.WallpaperFavoriteList.AddRange(user.WallpaperFavoriteList);
        response.OwnedLobbyDecoBackgroundIdList.AddRange(user.LobbyDecoBackgroundList);

        response.Playlists.AddRange(user.PlayLists);
        if (user.FavoriteSongs != null)
        {
            response.FavoriteSongs = user.FavoriteSongs;
        }

        response.JukeboxIdList.AddRange(JukeboxUtils.GetUnlockedSongs(user));

        await WriteDataAsync(response);
    }
}
