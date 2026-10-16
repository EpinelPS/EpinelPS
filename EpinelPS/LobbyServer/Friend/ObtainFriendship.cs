using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Friend;

[GameRequest("/friend/obtainfriendship")]
public class ObtainFriendship : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainFriendshipPoint req = await ReadData<ReqObtainFriendshipPoint>();
        User user = GetUser();

        user.AddCurrency(CurrencyType.FriendshipPoint, 2);

        ResObtainFriendshipPoint response = new()
        {
            Result = FriendshipPointResult.Success
        };

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
