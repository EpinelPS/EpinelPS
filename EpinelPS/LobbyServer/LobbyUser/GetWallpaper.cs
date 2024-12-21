using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetWallpaper")]
    public class GetWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetWallpaper>();
            var response = new ResGetWallpaper();
            var user = GetUser();


            response.WallpaperList.AddRange(user.WallpaperList);
            response.WallpaperPlaylistList.AddRange(user.WallpaperPlaylistList);
            response.WallpaperJukeboxList.AddRange(user.WallpaperJukeboxList);
            response.WallpaperBackgroundList.AddRange(user.WallpaperBackground);
            response.WallpaperFavoriteList.AddRange(user.WallpaperFavoriteList);
            response.OwnedLobbyDecoBackgroundIdList.AddRange(user.LobbyDecoBackgroundList);

            // TODO: JukeboxIdList

            await WriteDataAsync(response);
        }
    }
}
