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
        foreach (MessengerConditionTriggerRecord condition in GameData.Instance.MessageConditions.Values)
        {
            if (condition.MessageType != MessageType.Message || string.IsNullOrEmpty(condition.Tid))
                continue;

            // Empty conditions are intentionally not auto-created. They are
            // commonly room/group definitions and must be entered explicitly.
            if (condition.TriggerList == null || condition.TriggerList.Count == 0 ||
                !condition.TriggerList.Any(item =>
                    item.Trigger == currentTrigger.Type &&
                    item.ConditionId == currentTrigger.ConditionId &&
                    currentTrigger.Value >= item.ConditionValue))
                continue;

            if (!MessengerTriggerUtils.IsTriggerListSatisfied(user, condition.TriggerList))
                continue;

            CreateOpener(user, condition);
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

    private static bool CreateOpener(User user, MessengerConditionTriggerRecord condition)
    {
        if (user.MessengerData.Any(message => message.ConversationId == condition.Tid))
            return false;

        KeyValuePair<string, MessengerDialogRecord> opener = GameData.Instance.Messages.FirstOrDefault(item =>
            item.Value.ConversationId == condition.Tid && item.Value.IsOpener);
        if (opener.Value == null || !MessengerAccessValidator.IsRoomUnlockSatisfied(user, opener.Value.RoomId))
            return false;

        user.CreateMessage(opener.Value);
        Logging.WriteLine($"[Messenger] Created opener from trigger: user={user.ID}, Tid={condition.Tid}, RoomId={opener.Value.RoomId}", LogType.Info);
        return true;
    }
}
