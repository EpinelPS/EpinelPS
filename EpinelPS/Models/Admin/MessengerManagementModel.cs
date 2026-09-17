namespace EpinelPS.Models.Admin;

public class MessengerManagementModel
{
    public ulong UserId { get; init; }
    public int MessageCount { get; init; }
    public int ConversationCount { get; init; }
    public int RoomCount { get; init; }
    public List<MessengerMessageModel> Messages { get; init; } = [];
    public List<MessengerRepairAuditEntry> History { get; init; } = [];
}

public class MessengerMessageModel
{
    public long Seq { get; init; }
    public string ConversationId { get; init; } = "";
    public string MessageId { get; init; } = "";
    public string RoomId { get; init; } = "";
    public int State { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public bool StaticDialogFound { get; init; }
    public bool IsOpener { get; init; }
    public int? ConditionId { get; init; }
    public string MessageType { get; init; } = "";
    public int TriggerCount { get; init; }
    public bool? TriggerListSatisfied { get; init; }
    public bool RoomUnlocked { get; init; }
    public bool CanSafelyDelete { get; init; }
    public string Status { get; init; } = "";
}

public class CreateMessengerMessageModel
{
    public string DialogId { get; set; } = "";
    public int State { get; set; }
}

public class UpdateMessengerMessageModel
{
    public long Seq { get; set; }
    public int State { get; set; }
}
