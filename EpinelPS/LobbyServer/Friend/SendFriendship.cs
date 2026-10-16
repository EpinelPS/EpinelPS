using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Friend;

[GameRequest("/friend/sendfriendship")]
public class SendFriendship : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSendFriendshipPoint req = await ReadData<ReqSendFriendshipPoint>();
        User user = GetUser();

        // Increment friendship point sending progress for single friend action
        user.AddTrigger(Trigger.SendFriendShipPoint, 1);

        ResSendFriendshipPoint response = new()
        {
            Result = FriendshipPointResult.Success
        };

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
