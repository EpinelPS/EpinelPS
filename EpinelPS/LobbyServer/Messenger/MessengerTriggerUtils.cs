using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Messenger;

internal static class MessengerTriggerUtils
{
    public static bool IsTriggerListSatisfied(User user, List<TriggerData>? triggerList)
    {
        if (triggerList == null)
            return true;

        return triggerList.All(trigger =>
            trigger.Trigger == Data.Trigger.None || CheckTriggerCondition(user, trigger));
    }

    public static bool CheckTriggerCondition(User user, TriggerData trigger)
    {
        return GameContext.Instance.Triggers.Any(t =>
            t.UserId == user.ID &&
            t.Type == trigger.Trigger &&
            t.ConditionId == trigger.ConditionId &&
            t.Value >= trigger.ConditionValue);
    }
}
