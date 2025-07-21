using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/wallpaper/buy")]
    public class BuyWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBuyLobbyDecoBackground req = await ReadData<ReqBuyLobbyDecoBackground>();
            ResBuyLobbyDecoBackground response = new();
            User user = GetUser();
            
            user.LobbyDecoBackgroundList.Add(req.LobbyDecoBackgroundId);

            response.OwnedLobbyDecoBackgroundIdList.Add(user.LobbyDecoBackgroundList);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
