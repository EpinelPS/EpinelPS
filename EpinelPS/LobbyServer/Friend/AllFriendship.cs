using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Friend;

[GameRequest("/friend/allfriendship")]
public class AllFriendship : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAllFriendshipPoint req = await ReadData<ReqAllFriendshipPoint>();
        User user = GetUser();

        // Generously grant daily social friendship points on "Send & Receive All"
        const int exchangeCount = 30;
        user.AddCurrency(CurrencyType.FriendshipPoint, exchangeCount);

        // Directly satisfies Daily 10006 (Send 1 point) and Weekly 20008 (Send 10 points)
        user.AddTrigger(Trigger.SendFriendShipPoint, 10);

        ResAllFriendshipPoint response = new()
        {
            SendCount = exchangeCount,
            ReceiveCount = exchangeCount,
            Result = FriendshipPointResult.Success
        };

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
