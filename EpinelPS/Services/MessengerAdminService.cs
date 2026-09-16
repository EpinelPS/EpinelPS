using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Models.Admin;

namespace EpinelPS.Services;

/// <summary>
/// Read-only diagnostics for the admin Messenger page. This service does not
/// write MessengerData, triggers, rooms, quests, or character progression.
/// </summary>
public sealed class MessengerAdminService(GameContext database)
{
    public MessengerInspectionModel BuildInspection(User user)
    {
        List<TriggerModelNew> triggers = database.Triggers
            .Where(trigger => trigger.UserId == user.ID)
            .ToList();

        List<MessengerRecordInspection> records = user.MessengerData
            .OrderByDescending(message => message.Seq)
            .Select(message => Describe(user, message, triggers))
            .ToList();

        return new MessengerInspectionModel
        {
            UserId = user.ID,
            MessageCount = records.Count,
            ConversationCount = records.Select(record => record.ConversationId).Distinct().Count(),
            RoomCount = records.Where(record => !string.IsNullOrEmpty(record.RoomId)).Select(record => record.RoomId).Distinct().Count(),
            Records = records
        };
    }

    private static MessengerRecordInspection Describe(User user, NetMessage message, List<TriggerModelNew> triggers)
    {
        bool dialogFound = GameData.Instance.Messages.TryGetValue(message.MessageId, out MessengerDialogRecord? dialog);
        MessengerConditionTriggerRecord? condition = GameData.Instance.MessageConditions.Values
            .FirstOrDefault(candidate => candidate.Tid == message.ConversationId);

        string roomId = dialog?.RoomId ?? "";
        bool? roomUnlocked = null;
        string roomCondition = "Static room not found";
        if (!string.IsNullOrEmpty(roomId) && GameData.Instance.MessengerRooms.TryGetValue(roomId, out MessengerRoomRecord? room))
        {
            roomCondition = DescribeRoomCondition(room);
            roomUnlocked = IsRoomUnlocked(user, room);
        }

        string conditionStatus = DescribeConditionStatus(user, condition, triggers);
        return new MessengerRecordInspection
        {
            Seq = message.Seq,
            ConversationId = message.ConversationId,
            MessageId = message.MessageId,
            State = message.State,
            CreatedAtUtc = ToUtcDateTime(message.CreatedAt),
            StaticDialogFound = dialogFound,
            IsOpener = dialog?.IsOpener ?? false,
            RoomId = roomId,
            RoomCondition = roomCondition,
            RoomUnlocked = roomUnlocked,
            ConditionId = condition?.Id,
            ConditionType = condition?.MessageType.ToString() ?? "No condition",
            TriggerCount = condition?.TriggerList?.Count ?? 0,
            ConditionStatus = conditionStatus,
            StaticStatus = dialogFound ? "Found" : "Missing"
        };
    }

    private static string DescribeConditionStatus(User user, MessengerConditionTriggerRecord? condition, List<TriggerModelNew> triggers)
    {
        if (condition == null)
            return "No condition record";

        if (condition.MessageType is MessageType.RandomMessage or MessageType.DailyMessage)
            return user.PickedMessages.Any(pick => pick.ConversationId == condition.Tid) ? "Picked" : "Pick required";

        if (condition.TriggerList == null || condition.TriggerList.All(trigger => trigger.Trigger == Trigger.None))
            return "No trigger requirement";

        return condition.TriggerList.All(trigger => trigger.Trigger == Trigger.None || triggers.Any(row =>
            row.Type == trigger.Trigger &&
            row.ConditionId == trigger.ConditionId &&
            row.Value >= trigger.ConditionValue))
            ? "Satisfied"
            : "Not satisfied";
    }

    private static string DescribeRoomCondition(MessengerRoomRecord room)
    {
        List<string> conditions = [];
        if (room.UnlockConditionSquad != Squad.None)
            conditions.Add($"Squad: {room.UnlockConditionSquad}");
        if (room.UnlockConditionCharacter != 0)
            conditions.Add($"Character name code: {room.UnlockConditionCharacter}");

        return conditions.Count == 0 ? "No room requirement" : string.Join(" · ", conditions);
    }

    private static bool IsRoomUnlocked(User user, MessengerRoomRecord room)
    {
        bool squadSatisfied = room.UnlockConditionSquad == Squad.None || user.Characters.Any(character =>
            GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? record) &&
            record.Squad == room.UnlockConditionSquad);

        bool characterSatisfied = room.UnlockConditionCharacter == 0 || user.Characters.Any(character =>
            GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? record) &&
            record.NameCode == room.UnlockConditionCharacter);

        return squadSatisfied && characterSatisfied;
    }

    private static DateTime? ToUtcDateTime(long ticks)
    {
        try
        {
            return ticks > 0 ? new DateTime(ticks, DateTimeKind.Utc) : null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
