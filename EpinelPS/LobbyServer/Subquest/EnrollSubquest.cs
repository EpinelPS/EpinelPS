using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Messenger;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Subquest;

[GameRequest("/subquest/enrollment")]
public class EnrollSubquest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnrollmentSubQuest req = await ReadData<ReqEnrollmentSubQuest>();
        User user = GetUser();

        ResEnrollmentSubQuest response = new();

        if (!GameData.Instance.Subquests.TryGetValue(req.SubquestId, out SubQuestRecord? subQuest))
        {
            Logging.Warn($"[EnrollSubquest] Subquest {req.SubquestId} not found.");
            await WriteDataAsync(response);
            return;
        }

        if (subQuest.BeforeSubQuestId > 0 &&
            (!user.SubQuestData.TryGetValue(subQuest.BeforeSubQuestId, out bool previousCompleted) || !previousCompleted))
        {
            Logging.WriteLine($"[EnrollSubquest] Subquest {req.SubquestId} prerequisite {subQuest.BeforeSubQuestId} not completed for user {user.ID}.", LogType.Debug);
        }

        if (!MessengerTriggerUtils.IsTriggerListSatisfied(user, subQuest.TriggerList))
        {
            Logging.WriteLine($"[EnrollSubquest] Subquest {req.SubquestId} conditions not strictly satisfied for user {user.ID}, enrolling per client request.", LogType.Debug);
        }

        bool isReceived = false;
        if (user.SubQuestData.TryGetValue(req.SubquestId, out bool existingState))
        {
            isReceived = existingState;
        }
        else
        {
            user.SetSubQuest(req.SubquestId, false);
        }

        // Ensure opener message exists in user's MessengerData so player can converse in BlaBla
        if (!string.IsNullOrEmpty(subQuest.ConversationId) && !user.MessengerData.Any(m => m.ConversationId == subQuest.ConversationId))
        {
            KeyValuePair<string, MessengerDialogRecord> opener = GameData.Instance.Messages.FirstOrDefault(item =>
                item.Value.ConversationId == subQuest.ConversationId && item.Value.IsOpener);
            if (opener.Value == null)
            {
                opener = GameData.Instance.Messages
                    .Where(item => item.Value.ConversationId == subQuest.ConversationId)
                    .OrderBy(item => item.Key)
                    .FirstOrDefault();
            }
            if (opener.Value != null)
            {
                user.CreateMessage(opener.Value);
            }
        }

        response.SubquestData = new NetSubQuestData()
        {
            CreatedAt = DateTime.UtcNow.Ticks,
            IsReceived = isReceived,
            SubQuestId = req.SubquestId
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
