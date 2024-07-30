using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/User/GetWallpaper")]
    public class GetWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetWallpaper>();
            var response = new ResGetWallpaper();
            response.WallpaperList.AddRange(GetUser().WallpaperList);

            await WriteDataAsync(response);
        }
    }
}
