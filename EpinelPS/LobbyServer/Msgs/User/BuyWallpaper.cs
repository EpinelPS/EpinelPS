using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/user/wallpaper/buy")]
    public class BuyWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqBuyLobbyDecoBackground>();
            var response = new ResBuyLobbyDecoBackground();
            var user = GetUser();
            
            user.LobbyDecoBackgroundList.Add(req.LobbyDecoBackgroundId);

            response.OwnedLobbyDecoBackgroundIdList.Add(user.LobbyDecoBackgroundList);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
