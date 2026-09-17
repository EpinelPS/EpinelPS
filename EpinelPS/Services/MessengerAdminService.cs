using EpinelPS.Data;
using EpinelPS.LobbyServer.Messenger;
using EpinelPS.Models.Admin;

namespace EpinelPS.Services;

public class MessengerAdminService
{
    public MessengerManagementModel BuildModel(User user)
    {
        Dictionary<string, int> conversationCounts = user.MessengerData
            .GroupBy(message => message.ConversationId)
            .ToDictionary(group => group.Key, group => group.Count());

        List<MessengerMessageModel> messages = user.MessengerData
            .OrderByDescending(message => message.Seq)
            .Select(message => Describe(user, message, conversationCounts[message.ConversationId]))
            .ToList();

        return new MessengerManagementModel
        {
            UserId = user.ID,
            MessageCount = messages.Count,
            ConversationCount = conversationCounts.Count,
            RoomCount = messages.Where(message => !string.IsNullOrEmpty(message.RoomId)).Select(message => message.RoomId).Distinct().Count(),
            Messages = messages,
            History = user.MessengerRepairHistory.OrderByDescending(entry => entry.CreatedAtUtc).ToList()
        };
    }

    public MessengerMessageModel Describe(User user, NetMessage message)
    {
        int count = user.MessengerData.Count(item => item.ConversationId == message.ConversationId);
        return Describe(user, message, count);
    }

    public bool TryCreate(User user, CreateMessengerMessageModel input, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(input.DialogId) || !GameData.Instance.Messages.TryGetValue(input.DialogId, out MessengerDialogRecord? dialog))
        {
            error = "The dialog ID does not exist in MessengerDialogTable.";
            return false;
        }

        if (user.MessengerData.Any(message => message.MessageId == dialog.Id))
        {
            error = "This dialog message already exists for the user.";
            return false;
        }

        NetMessage message = user.CreateMessage(dialog, input.State);
        AddAudit(user, "Created", message, "Created manually from MessengerDialogTable. No trigger was added.");
        return true;
    }

    public bool TryUpdateState(User user, UpdateMessengerMessageModel input, out string error)
    {
        error = "";
        if (input.State is < 0 or > 2)
        {
            error = "State must be 0, 1, or 2.";
            return false;
        }

        NetMessage? message = user.MessengerData.FirstOrDefault(item => item.Seq == input.Seq);
        if (message == null)
        {
            error = "The message no longer exists.";
            return false;
        }

        AddAudit(user, "State updated", message, $"State changed from {message.State} to {input.State}.");
        message.State = input.State;
        return true;
    }

    public bool TryDelete(User user, long seq, out string error)
    {
        error = "";
        NetMessage? message = user.MessengerData.FirstOrDefault(item => item.Seq == seq);
        if (message == null)
        {
            error = "The message no longer exists.";
            return false;
        }

        MessengerMessageModel description = Describe(user, message);
        if (!description.CanSafelyDelete)
        {
            error = "Only a lone opener in a currently locked room can be removed here. This record needs manual investigation.";
            return false;
        }

        AddAudit(user, "Deleted", message, description.Status);
        user.MessengerData.Remove(message);
        return true;
    }

    public bool TryRestore(User user, string auditId, out string error)
    {
        error = "";
        MessengerRepairAuditEntry? entry = user.MessengerRepairHistory.FirstOrDefault(item => item.Id == auditId && item.Action == "Deleted");
        if (entry == null)
        {
            error = "The deleted-message audit record was not found.";
            return false;
        }

        if (user.MessengerData.Any(message => message.Seq == entry.Message.Seq))
        {
            error = "A message with this sequence number already exists.";
            return false;
        }

        NetMessage restored = entry.Message.Clone();
        user.MessengerData.Add(restored);
        user.LastMessageId = Math.Max(user.LastMessageId, (ulong)restored.Seq + 1);
        AddAudit(user, "Restored", restored, $"Restored deleted audit entry {entry.Id}.");
        return true;
    }

    private static MessengerMessageModel Describe(User user, NetMessage message, int conversationMessageCount)
    {
        bool staticDialogFound = GameData.Instance.Messages.TryGetValue(message.MessageId, out MessengerDialogRecord? dialog);
        MessengerConditionTriggerRecord? condition = GameData.Instance.MessageConditions.Values
            .FirstOrDefault(item => item.Tid == message.ConversationId);
        string roomId = dialog?.RoomId ?? "";
        bool roomUnlocked = staticDialogFound && MessengerAccessValidator.IsRoomUnlockSatisfied(user, roomId);
        bool isOpener = dialog?.IsOpener ?? false;
        bool canSafelyDelete = staticDialogFound && isOpener && conversationMessageCount == 1 && !roomUnlocked;
        bool? triggerListSatisfied = condition == null
            ? null
            : condition.MessageType is MessageType.RandomMessage or MessageType.DailyMessage
                ? null
                : MessengerTriggerUtils.IsTriggerListSatisfied(user, condition.TriggerList);

        string status = !staticDialogFound
            ? "Static dialog is missing; do not delete automatically."
            : canSafelyDelete
                ? "Safe repair candidate: lone opener in a locked room."
                : conversationMessageCount > 1
                    ? "Conversation has progressed; manual investigation required."
                    : roomUnlocked
                        ? "Room is currently unlocked."
                        : "Not an opener; manual investigation required.";

        DateTime? createdAtUtc = message.CreatedAt > 0 ? new DateTime(message.CreatedAt, DateTimeKind.Utc) : null;
        return new MessengerMessageModel
        {
            Seq = message.Seq,
            ConversationId = message.ConversationId,
            MessageId = message.MessageId,
            RoomId = roomId,
            State = message.State,
            CreatedAtUtc = createdAtUtc,
            StaticDialogFound = staticDialogFound,
            IsOpener = isOpener,
            ConditionId = condition?.Id,
            MessageType = condition?.MessageType.ToString() ?? "—",
            TriggerCount = condition?.TriggerList?.Count ?? 0,
            TriggerListSatisfied = triggerListSatisfied,
            RoomUnlocked = roomUnlocked,
            CanSafelyDelete = canSafelyDelete,
            Status = status
        };
    }

    private static void AddAudit(User user, string action, NetMessage message, string note)
    {
        user.MessengerRepairHistory.Add(new MessengerRepairAuditEntry
        {
            Action = action,
            Note = note,
            Message = message.Clone()
        });
    }
}
