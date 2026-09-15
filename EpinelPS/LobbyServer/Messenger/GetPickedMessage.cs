using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/picked/get")]
public class GetPickedMessage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetPickedMessageList req = await ReadData<ReqGetPickedMessageList>();
        User user = GetUser();

        ResGetPickedMessageList response = new();

        // Only return picks from today (last 24 hours)
        DateTime cutoff = DateTime.UtcNow.AddHours(-24);

        foreach (NetPickedMessage pick in user.PickedMessages)
        {
            if (pick.CreatedAt != null && pick.CreatedAt.ToDateTime() > cutoff)
            {
                response.Data.Add(pick);
            }
        }

        Logging.WriteLine($"[Messenger] Picked/get: returning {response.Data.Count} picks for user {user.ID}", LogType.Debug);

        await WriteDataAsync(response);
    }
}
