using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/get")]
public class GetMessages : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMessages req = await ReadData<ReqGetMessages>();
        User user = GetUser();

        CheckAndCreateAvailableMessages(user);

        ResGetMessages response = new();

        IEnumerable<NetMessage> newMessages = user.MessengerData.Where(x => x.Seq >= req.Seq);

        foreach (NetMessage? item in newMessages)
        {
            response.Messages.Add(item);
        }

        await WriteDataAsync(response);
    }

    private void CheckAndCreateAvailableMessages(User user)
    {
        foreach (KeyValuePair<int, MessengerConditionTriggerRecord> messageCondition in GameData.Instance.MessageConditions)
        {
            int conditionId = messageCondition.Key;
            MessengerConditionTriggerRecord msgCondition = messageCondition.Value;

            Logging.WriteLine($"[Messenger] Checking condition {conditionId}, Tid={msgCondition.Tid}, MessageType={msgCondition.MessageType}, TriggerCount={msgCondition.TriggerList?.Count ?? 0}", LogType.Debug);

            bool isPicked = false;
            if (msgCondition.MessageType == MessageType.RandomMessage || msgCondition.MessageType == MessageType.DailyMessage)
            {
                isPicked = user.PickedMessages.Any(p => p.ConversationId == msgCondition.Tid);
                // For picked messages, skip the trigger list check - the pick itself is the authorization
                if (!isPicked)
                    continue;
            }
            else if (!IsTriggerListSatisfied(user, msgCondition.TriggerList))
            {
                Logging.WriteLine($"[Messenger] Condition {conditionId} NOT satisfied for user {user.ID}", LogType.Debug);
                LogUnsatisfiedTriggers(user, msgCondition.TriggerList);
                continue;
            }

            bool messageExists = user.MessengerData.Any(m => m.ConversationId == msgCondition.Tid);
            if (!messageExists)
            {
                KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.FirstOrDefault(x =>
                    x.Value.ConversationId == msgCondition.Tid && x.Value.IsOpener);

                if (conversation.Value != null)
                {
                    Logging.WriteLine($"[Messenger] Creating message for condition {conditionId}, Tid={msgCondition.Tid}, RoomId={conversation.Value.RoomId}, user={user.ID}", LogType.Info);
                    user.CreateMessage(conversation.Value);
                }
                else
                {
                    Logging.WriteLine($"[Messenger] No opener found for Tid={msgCondition.Tid}", LogType.Warning);
                }
            }
        }
    }

    private void LogUnsatisfiedTriggers(User user, List<TriggerData>? triggerList)
    {
        if (triggerList == null) return;

        foreach (TriggerData trigger in triggerList)
        {
            if (trigger.Trigger == Data.Trigger.None) continue;

            bool satisfied = MessengerTriggerUtils.CheckTriggerCondition(user, trigger);
            if (!satisfied)
            {
                Logging.WriteLine($"[Messenger]   UNSATISFIED: Trigger={trigger.Trigger}, ConditionId={trigger.ConditionId}, ConditionValue={trigger.ConditionValue}", LogType.Debug);
            }
        }
    }

    private bool IsTriggerListSatisfied(User user, List<TriggerData>? triggerList)
    {
        return MessengerTriggerUtils.IsTriggerListSatisfied(user, triggerList);
    }
}
