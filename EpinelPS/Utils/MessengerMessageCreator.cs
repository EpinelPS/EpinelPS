using EpinelPS.Data;
using EpinelPS.Database;
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
    public static void OnTriggerAdded(User user, TriggerModelNew currentTrigger, bool logToConsole = true)
    {
        int sameTypeCandidates = 0;
        int exactCandidates = 0;
        int satisfiedCandidates = 0;
        int createdCount = 0;

        foreach (MessengerConditionTriggerRecord condition in GameData.Instance.MessageConditions.Values.OrderBy(c => c.Id))
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
                Logging.WriteLine($"[Messenger] Trigger candidate not satisfied: user={user.ID}, Trigger={currentTrigger.Type}, ConditionId={currentTrigger.ConditionId}, MessengerCondition={condition.Id}, Tid={condition.Tid}", LogType.Debug, logToConsole);
                continue;
            }

            satisfiedCandidates++;
            if (CreateOpener(user, condition, logToConsole))
                createdCount++;
        }

        if (sameTypeCandidates > 0)
        {
            Logging.WriteLine($"[Messenger] Trigger evaluation: user={user.ID}, Trigger={currentTrigger.Type}, ConditionId={currentTrigger.ConditionId}, Value={currentTrigger.Value}, SameType={sameTypeCandidates}, Exact={exactCandidates}, Satisfied={satisfiedCandidates}, Created={createdCount}", LogType.Info, logToConsole);
        }

        CreateEligibleSubquestOpeners(user, logToConsole);
    }

    public static bool CreateForCondition(User user, MessengerConditionTriggerRecord condition, bool logToConsole = true)
    {
        if (condition.MessageType is MessageType.RandomMessage or MessageType.DailyMessage)
            return false;
        if (string.IsNullOrEmpty(condition.Tid))
            return false;
        if (condition.TriggerList == null || condition.TriggerList.Count == 0 ||
            !MessengerTriggerUtils.IsTriggerListSatisfied(user, condition.TriggerList))
            return false;

        return CreateOpener(user, condition, logToConsole);
    }

    /// <summary>
    /// Re-evaluates fixed conversations after a change which can unlock a
    /// room without itself appearing in the conversation's TriggerList (for
    /// example, granting the first character from a Squad through admin).
    /// This creates only normal conversations whose recorded progression and
    /// room requirements are already satisfied; it never writes triggers.
    /// Also reconciles eligible subquest openers and enrolls them.
    /// </summary>
    public static int CreateAllEligibleOpeners(User user, bool logToConsole = true)
    {
        int createdCount = 0;

        foreach (MessengerConditionTriggerRecord condition in GameData.Instance.MessageConditions.Values.OrderBy(c => c.Id))
        {
            if (CreateForCondition(user, condition, logToConsole))
                createdCount++;
        }

        createdCount += CreateEligibleSubquestOpeners(user, logToConsole);

        if (createdCount > 0)
        {
            JsonDb.Save();
        }

        Logging.WriteLine($"[Messenger] Eligible opener reconciliation: user={user.ID}, Created={createdCount}", LogType.Info, logToConsole);
        return createdCount;
    }

    /// <summary>
    /// Evaluates all subquests and creates starting openers in MessengerData (and enrolls in SubQuestData)
    /// for any subquest whose stage/quest triggers and prerequisite subquests are satisfied.
    /// Preserves subquests for players to activate and play in BlaBla.
    /// </summary>
    public static int CreateEligibleSubquestOpeners(User user, bool logToConsole = true)
    {
        int createdCount = 0;

        foreach (SubQuestRecord subQuest in GameData.Instance.Subquests.Values.OrderBy(s => s.Id))
        {
            if (string.IsNullOrEmpty(subQuest.ConversationId))
                continue;

            // If subquest is already completed, do not re-create
            if (user.SubQuestData.TryGetValue(subQuest.Id, out bool completed) && completed)
                continue;

            // If opener message already exists in MessengerData, do not re-create
            if (user.MessengerData.Any(message => message.ConversationId == subQuest.ConversationId))
                continue;

            // Prerequisite check: If BeforeSubQuestId > 0, it must be completed
            if (subQuest.BeforeSubQuestId > 0)
            {
                if (!user.SubQuestData.TryGetValue(subQuest.BeforeSubQuestId, out bool prevDone) || !prevDone)
                    continue;
            }

            // Do not auto-unlock dummy subquests with no triggers and no prerequisite
            bool hasMeaningfulTrigger = subQuest.TriggerList != null && subQuest.TriggerList.Any(t => t.Trigger != Trigger.None);
            if (!hasMeaningfulTrigger && subQuest.BeforeSubQuestId == 0)
                continue;

            // Condition triggers check (e.g. CampaignClear of required stage)
            if (!MessengerTriggerUtils.IsTriggerListSatisfied(user, subQuest.TriggerList))
                continue;

            KeyValuePair<string, MessengerDialogRecord> opener = GameData.Instance.Messages.FirstOrDefault(item =>
                item.Value.ConversationId == subQuest.ConversationId && item.Value.IsOpener);
            if (opener.Value == null)
            {
                opener = GameData.Instance.Messages
                    .Where(item => item.Value.ConversationId == subQuest.ConversationId)
                    .OrderBy(item => item.Key)
                    .FirstOrDefault();
            }
            if (opener.Value == null)
                continue;

            if (!MessengerAccessValidator.IsRoomUnlockSatisfied(user, opener.Value.RoomId))
                continue;

            // Avoid spawning into a room that already has an active unread conversation
            if (!string.IsNullOrEmpty(opener.Value.RoomId) && HasActiveUnclearedConversationInRoom(user, opener.Value.RoomId))
                continue;

            // Create the starting opener message in BlaBla
            user.CreateMessage(opener.Value);

            createdCount++;
            Logging.WriteLine($"[Messenger] Created subquest opener: user={user.ID}, SubQuestId={subQuest.Id}, Tid={subQuest.ConversationId}", LogType.Info, logToConsole);
        }

        return createdCount;
    }


    /// <summary>
    /// Checks whether a room already has an active conversation in MessengerData that
    /// has not been entered/cleared by the user. Prevents dialogue interleaving and soft-locks.
    /// </summary>
    public static bool HasActiveUnclearedConversationInRoom(User user, string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return false;

        var existingConvIds = user.MessengerData
            .Select(m => m.ConversationId)
            .Distinct()
            .ToList();

        if (existingConvIds.Count == 0)
            return false;

        using var ctx = GameContext.CreateNew();
        var clearedConditionIds = ctx.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.MessageClear)
            .Select(t => t.ConditionId)
            .ToHashSet();

        foreach (string convId in existingConvIds)
        {
            var dialog = GameData.Instance.Messages.Values.FirstOrDefault(d => d.ConversationId == convId);
            if (dialog == null || dialog.RoomId != roomId)
                continue;

            var cond = GameData.Instance.MessageConditions.Values.FirstOrDefault(c => c.Tid == convId);
            if (cond != null)
            {
                bool isCleared = clearedConditionIds.Contains(cond.Id);
                if (!isCleared)
                {
                    bool hasProgressed = user.MessengerData.Any(m => m.ConversationId == convId && m.State != 0);
                    if (!hasProgressed)
                    {
                        return true;
                    }
                }
            }
            else
            {
                var sub = GameData.Instance.Subquests.Values.FirstOrDefault(s => s.ConversationId == convId);
                if (sub != null)
                {
                    bool isDone = user.SubQuestData.TryGetValue(sub.Id, out bool done) && done;
                    if (!isDone)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool CreateOpener(User user, MessengerConditionTriggerRecord condition, bool logToConsole = true)
    {
        if (user.MessengerData.Any(message => message.ConversationId == condition.Tid))
        {
            Logging.WriteLine($"[Messenger] Opener already exists: user={user.ID}, MessengerCondition={condition.Id}, Tid={condition.Tid}", LogType.Debug, logToConsole);
            return false;
        }

        KeyValuePair<string, MessengerDialogRecord> opener = GameData.Instance.Messages.FirstOrDefault(item =>
            item.Value.ConversationId == condition.Tid && item.Value.IsOpener);
        if (opener.Value == null)
        {
            Logging.WriteLine($"[Messenger] Opener is missing from static data: user={user.ID}, MessengerCondition={condition.Id}, Tid={condition.Tid}", LogType.Warning, logToConsole);
            return false;
        }

        if (!MessengerAccessValidator.IsRoomUnlockSatisfied(user, opener.Value.RoomId))
        {
            Logging.WriteLine($"[Messenger] Opener room is locked: user={user.ID}, MessengerCondition={condition.Id}, Tid={condition.Tid}, RoomId={opener.Value.RoomId}", LogType.Debug, logToConsole);
            return false;
        }

        // Avoid spawning multiple active unread conversations in the same room simultaneously.
        // A room should only have one active conversation at a time to prevent UI breaks.
        if (!string.IsNullOrEmpty(opener.Value.RoomId) && HasActiveUnclearedConversationInRoom(user, opener.Value.RoomId))
        {
            Logging.WriteLine($"[Messenger] Room {opener.Value.RoomId} already has an active unread conversation; postponing Tid={condition.Tid}", LogType.Debug, logToConsole);
            return false;
        }

        user.CreateMessage(opener.Value);
        Logging.WriteLine($"[Messenger] Created opener from trigger: user={user.ID}, Tid={condition.Tid}, RoomId={opener.Value.RoomId}", LogType.Info, logToConsole);
        return true;
    }

}
