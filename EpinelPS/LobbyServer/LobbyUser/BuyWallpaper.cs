using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/wallpaper/buy")]
public class BuyWallpaper : LobbyMessage
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
