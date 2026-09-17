using EpinelPS.Data;
using EpinelPS.LobbyServer.Messenger;
using EpinelPS.Models;

namespace EpinelPS.Utils;

/// <summary>
/// Creates Messenger opener records when a real gameplay event makes a fixed
/// conversation eligible. This is deliberately called from event handlers,
/// never from /messenger/get.
/// </summary>
public static class MessengerMessageCreator
{
    public static void OnTriggerAdded(User user, TriggerModelNew currentTrigger)
    {
        int sameTypeCandidates = 0;
        int exactCandidates = 0;
        int satisfiedCandidates = 0;
        int createdCount = 0;

        foreach (MessengerConditionTriggerRecord condition in GameData.Instance.MessageConditions.Values)
        {
            if (condition.MessageType != MessageType.Message || string.IsNullOrEmpty(condition.Tid))
                continue;

            // Empty conditions are intentionally not auto-created. They are
            // commonly room/group definitions and must be entered explicitly.
            if (condition.TriggerList == null || condition.TriggerList.Count == 0)
                continue;

            if (!condition.TriggerList.Any(item => item.Trigger == currentTrigger.Type))
                continue;

            sameTypeCandidates++;

            if (!condition.TriggerList.Any(item =>
                    item.Trigger == currentTrigger.Type &&
                    item.ConditionId == currentTrigger.ConditionId &&
                    currentTrigger.Value >= item.ConditionValue))
                continue;

            exactCandidates++;
            if (!MessengerTriggerUtils.IsTriggerListSatisfied(user, condition.TriggerList))
            {
                Logging.WriteLine($"[Messenger] Trigger candidate not satisfied: user={user.ID}, Trigger={currentTrigger.Type}, ConditionId={currentTrigger.ConditionId}, MessengerCondition={condition.Id}, Tid={condition.Tid}", LogType.Debug);
                continue;
            }

            satisfiedCandidates++;
            if (CreateOpener(user, condition))
                createdCount++;
        }

        if (sameTypeCandidates > 0)
        {
            Logging.WriteLine($"[Messenger] Trigger evaluation: user={user.ID}, Trigger={currentTrigger.Type}, ConditionId={currentTrigger.ConditionId}, Value={currentTrigger.Value}, SameType={sameTypeCandidates}, Exact={exactCandidates}, Satisfied={satisfiedCandidates}, Created={createdCount}", LogType.Info);
        }
    }

    public static bool CreateForCondition(User user, MessengerConditionTriggerRecord condition)
    {
        if (condition.MessageType is MessageType.RandomMessage or MessageType.DailyMessage)
            return false;
        if (string.IsNullOrEmpty(condition.Tid))
            return false;
        if (condition.TriggerList == null || condition.TriggerList.Count == 0 ||
            !MessengerTriggerUtils.IsTriggerListSatisfied(user, condition.TriggerList))
            return false;

        return CreateOpener(user, condition);
    }

    /// <summary>
    /// Re-evaluates fixed conversations after a change which can unlock a
    /// room without itself appearing in the conversation's TriggerList (for
    /// example, granting the first character from a Squad through admin).
    /// This creates only normal conversations whose recorded progression and
    /// room requirements are already satisfied; it never writes triggers.
    /// </summary>
    public static int CreateAllEligibleOpeners(User user)
    {
        int createdCount = 0;

        foreach (MessengerConditionTriggerRecord condition in GameData.Instance.MessageConditions.Values)
        {
            if (CreateForCondition(user, condition))
                createdCount++;
        }

        Logging.WriteLine($"[Messenger] Eligible opener reconciliation: user={user.ID}, Created={createdCount}", LogType.Info);
        return createdCount;
    }

    private static bool CreateOpener(User user, MessengerConditionTriggerRecord condition)
    {
        if (user.MessengerData.Any(message => message.ConversationId == condition.Tid))
        {
            Logging.WriteLine($"[Messenger] Opener already exists: user={user.ID}, MessengerCondition={condition.Id}, Tid={condition.Tid}", LogType.Debug);
            return false;
        }

        KeyValuePair<string, MessengerDialogRecord> opener = GameData.Instance.Messages.FirstOrDefault(item =>
            item.Value.ConversationId == condition.Tid && item.Value.IsOpener);
        if (opener.Value == null)
        {
            Logging.WriteLine($"[Messenger] Opener is missing from static data: user={user.ID}, MessengerCondition={condition.Id}, Tid={condition.Tid}", LogType.Warning);
            return false;
        }

        if (!MessengerAccessValidator.IsRoomUnlockSatisfied(user, opener.Value.RoomId))
        {
            Logging.WriteLine($"[Messenger] Opener room is locked: user={user.ID}, MessengerCondition={condition.Id}, Tid={condition.Tid}, RoomId={opener.Value.RoomId}", LogType.Debug);
            return false;
        }

        user.CreateMessage(opener.Value);
        Logging.WriteLine($"[Messenger] Created opener from trigger: user={user.ID}, Tid={condition.Tid}, RoomId={opener.Value.RoomId}", LogType.Info);
        return true;
    }
}
