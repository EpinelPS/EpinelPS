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
        CheckAndEnrollSubQuests(user);

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
        // Retroactively add missing ObtainCharacter triggers for characters the user has
        bool hasAddedNew = false;
        bool hasAnySsr = false;
        foreach (CharacterModel character in user.Characters)
        {
            if (GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? charData))
            {
                if (charData.OriginalRare == OriginalRareType.SSR)
                    hasAnySsr = true;

                bool hasObtain = GameContext.Triggers.Any(t =>
                    t.UserId == user.ID &&
                    t.Type == Trigger.ObtainCharacter &&
                    t.ConditionId == charData.NameCode);

                if (!hasObtain)
                {
                    Logging.WriteLine($"[Messenger] Retroactively adding ObtainCharacter trigger for NameCode={charData.NameCode}", LogType.Info);
                    user.AddTrigger(Trigger.ObtainCharacter, 1, charData.NameCode);
                    hasAddedNew = true;
                }
            }
        }
        if (hasAddedNew)
        {
            bool hasNew = GameContext.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.ObtainCharacterNew);
            if (!hasNew) user.AddTrigger(Trigger.ObtainCharacterNew, 1);
        }
        // Check ObtainCharacterSSR separately - don't depend on hasAddedNew
        if (hasAnySsr)
        {
            bool hasSsr = GameContext.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.ObtainCharacterSSR);
            if (!hasSsr)
            {
                Logging.WriteLine("[Messenger] Retroactively adding ObtainCharacterSSR trigger", LogType.Info);
                user.AddTrigger(Trigger.ObtainCharacterSSR, 1);
            }
        }

        

        // Retroactively add missing MainQuestClear triggers needed by messenger conditions
        foreach (KeyValuePair<int, MessengerConditionTriggerRecord> condKv in GameData.Instance.MessageConditions)
        {
            if (condKv.Value.TriggerList == null) continue;
            foreach (TriggerData trigger in condKv.Value.TriggerList)
            {
                if (trigger.Trigger == Trigger.MainQuestClear && trigger.ConditionId > 0)
                {
                    bool hasTrigger = GameContext.Triggers.Any(t =>
                        t.UserId == user.ID &&
                        t.Type == Trigger.MainQuestClear &&
                        t.ConditionId == trigger.ConditionId);
                    if (!hasTrigger)
                    {
                        Logging.WriteLine($"[Messenger] Retroactively adding MainQuestClear trigger for ConditionId={trigger.ConditionId}", LogType.Info);
                        user.AddTrigger(Trigger.MainQuestClear, 1, trigger.ConditionId);
                    }
                }
            }
        }

        // Build a lookup of Tid -> satisfied for cleanup
        var tidSatisfied = new Dictionary<string, bool>();
        foreach (KeyValuePair<int, MessengerConditionTriggerRecord> mc in GameData.Instance.MessageConditions)
        {
            tidSatisfied[mc.Value.Tid] = IsTriggerListSatisfied(user, mc.Value.TriggerList);
        }

        // Clean up stale messages whose conditions are no longer satisfied
        int removed = user.MessengerData.RemoveAll(m =>
        {
            if (m.ConversationId == null) return false;
            if (tidSatisfied.TryGetValue(m.ConversationId, out bool satisfied))
                return !satisfied;
            return false;
        });
        if (removed > 0)
            Logging.WriteLine($"[Messenger] Removed {removed} stale messages for user {user.ID}", LogType.Info);

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

            bool satisfied = CheckTriggerCondition(user, trigger);
            if (!satisfied)
            {
                Logging.WriteLine($"[Messenger]   UNSATISFIED: Trigger={trigger.Trigger}, ConditionId={trigger.ConditionId}, ConditionValue={trigger.ConditionValue}", LogType.Debug);
            }
        }
    }

    private void CheckAndEnrollSubQuests(User user)
    {
        foreach (KeyValuePair<int, SubQuestRecord> subQuestKv in GameData.Instance.Subquests)
        {
            SubQuestRecord subQuest = subQuestKv.Value;

            // Check prerequisite subquest
            if (subQuest.BeforeSubQuestId > 0)
            {
                if (!user.SubQuestData.TryGetValue(subQuest.BeforeSubQuestId, out bool prevCompleted) || !prevCompleted)
                    continue;
            }

            // Check trigger conditions
            if (!IsTriggerListSatisfied(user, subQuest.TriggerList))
                continue;

            // Auto-enroll if not already enrolled
            if (!user.SubQuestData.ContainsKey(subQuest.Id))
            {
                Logging.WriteLine($"[Messenger] Auto-enrolling subquest {subQuest.Id} for user {user.ID}", LogType.Info);
                user.SetSubQuest(subQuest.Id, false);
            }
        }
    }

    private bool IsTriggerListSatisfied(User user, List<TriggerData>? triggerList)
    {
        if (triggerList == null)
            return true;

        foreach (TriggerData trigger in triggerList)
        {
            if (trigger.Trigger == Data.Trigger.None)
                continue;

            if (!CheckTriggerCondition(user, trigger))
            {
                return false; // All conditions must be satisfied
            }
        }

        return true;
    }

    private bool CheckTriggerCondition(User user, TriggerData trigger)
    {
        return GameContext.Triggers.Any(t =>
            t.UserId == user.ID &&
            t.Type == trigger.Trigger &&
            t.ConditionId == trigger.ConditionId &&
            t.Value >= trigger.ConditionValue);
    }
}
