using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/get")]
    public class GetMessages : LobbyMsgHandler
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

        private static void CheckAndCreateAvailableMessages(User user)
        {
            foreach (KeyValuePair<int, MessengerConditionTriggerRecord> messageCondition in GameData.Instance.MessageConditions)
            {
                int conditionId = messageCondition.Key;
                MessengerConditionTriggerRecord msgCondition = messageCondition.Value;

                if (IsMessageConditionSatisfied(user, conditionId))
                {
                    bool messageExists = user.MessengerData.Any(m => m.ConversationId == msgCondition.Tid);
                    if (!messageExists)
                    {
                        KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.FirstOrDefault(x =>
                            x.Value.ConversationId == msgCondition.Tid && x.Value.IsOpener);

                        if (conversation.Value != null)
                        {
                            user.CreateMessage(conversation.Value);
                        }
                    }
                }
            }
        }

        private static bool IsMessageConditionSatisfied(User user, int conditionId)
        {
            if (!GameData.Instance.MessageConditions.TryGetValue(conditionId, out MessengerConditionTriggerRecord? msgCondition))
            {
                return false;
            }

            foreach (TriggerData trigger in msgCondition.TriggerList)
            {
                if (trigger.Trigger == Data.Trigger.None || trigger.ConditionId == 0)
                    continue;

                if (!CheckTriggerCondition(user, trigger))
                {
                    return false; // All conditions must be satisfied
                }
            }

            return true;
        }

        private static bool CheckTriggerCondition(User user, TriggerData trigger)
        {
            return user.Triggers.Any(t =>
                t.Type == trigger.Trigger &&
                t.ConditionId == trigger.ConditionId &&
                t.Value >= trigger.ConditionValue);
        }
    }
}
