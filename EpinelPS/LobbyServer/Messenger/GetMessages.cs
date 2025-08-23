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
            foreach (KeyValuePair<int, MessengerMsgConditionRecord> messageCondition in GameData.Instance.MessageConditions)
            {
                int conditionId = messageCondition.Key;
                MessengerMsgConditionRecord msgCondition = messageCondition.Value;

                if (IsMessageConditionSatisfied(user, conditionId))
                {
                    bool messageExists = user.MessengerData.Any(m => m.ConversationId == msgCondition.tid);
                    if (!messageExists)
                    {
                        KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.FirstOrDefault(x =>
                            x.Value.conversation_id == msgCondition.tid && x.Value.is_opener);

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
            if (!GameData.Instance.MessageConditions.TryGetValue(conditionId, out MessengerMsgConditionRecord? msgCondition))
            {
                return false;
            }

            foreach (MessengerConditionTriggerList trigger in msgCondition.trigger_list)
            {
                if (trigger.trigger == "None" || trigger.condition_id == 0)
                    continue;

                if (!CheckTriggerCondition(user, trigger))
                {
                    return false; // All conditions must be satisfied
                }
            }

            return true;
        }

        private static bool CheckTriggerCondition(User user, MessengerConditionTriggerList trigger)
        {
            TriggerType triggerType = ParseTriggerType(trigger.trigger);

            return user.Triggers.Any(t =>
                t.Type == triggerType &&
                t.ConditionId == trigger.condition_id &&
                t.Value >= trigger.condition_value);
        }

        private static TriggerType ParseTriggerType(string triggerString)
        {
            return triggerString switch
            {
                "ObtainCharacter" => TriggerType.ObtainCharacter,
                "MainQuestClear" => TriggerType.MainQuestClear,
                "MessageClear" => TriggerType.MessageClear,
                _ => TriggerType.None
            };
        }
    }
}
