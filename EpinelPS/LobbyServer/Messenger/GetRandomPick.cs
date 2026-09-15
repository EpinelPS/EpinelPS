using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.InteropServices;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/random/pick")]
public class GetRandomPick : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqPickTodayRandomMessage req = await ReadData<ReqPickTodayRandomMessage>();
        User user = GetUser();

        ResPickTodayRandomMessage response = new();

        // Filter eligible conditions that are MessageType.RandomMessage and user satisfies triggers
        List<int> eligibleIds = [];
        foreach (int conditionId in req.ConditionTriggerIds)
        {
            if (!GameData.Instance.MessageConditions.TryGetValue(conditionId, out MessengerConditionTriggerRecord? record))
                continue;

            if (record.MessageType != MessageType.RandomMessage)
                continue;

            // Check if user already has this conversation
            bool exists = user.MessengerData.Any(m => m.ConversationId == record.Tid);
            if (exists)
                continue;

            eligibleIds.Add(conditionId);
        }

        // Randomly select up to 3 random messages
        int maxPicks = Math.Min(3, eligibleIds.Count);
        Random.Shared.Shuffle(CollectionsMarshal.AsSpan(eligibleIds));
        List<int> selected = eligibleIds.Take(maxPicks).ToList();

        // Record picks
        DateTime now = DateTime.UtcNow;
        Timestamp nowTimestamp = Timestamp.FromDateTime(now);

        foreach (int conditionId in selected)
        {
            if (!GameData.Instance.MessageConditions.TryGetValue(conditionId, out MessengerConditionTriggerRecord? record))
                continue;

            // Check if already picked today
            bool alreadyPicked = user.PickedMessages.Any(p =>
                p.ConversationId == record.Tid &&
                p.CreatedAt != null &&
                p.CreatedAt.ToDateTime().Date == now.Date);

            if (alreadyPicked)
                continue;

            user.PickedMessages.Add(new NetPickedMessage
            {
                ConversationId = record.Tid,
                CreatedAt = nowTimestamp
            });

            Logging.WriteLine($"[Messenger] Random pick: user {user.ID} picked condition {conditionId} (Tid={record.Tid})", LogType.Info);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
