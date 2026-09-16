namespace EpinelPS.Models.Admin;

public sealed class MessengerInspectionModel
{
    public ulong UserId { get; init; }
    public int MessageCount { get; init; }
    public int ConversationCount { get; init; }
    public int RoomCount { get; init; }
    public List<MessengerRecordInspection> Records { get; init; } = [];
}

public sealed class MessengerRecordInspection
{
    public long Seq { get; init; }
    public string ConversationId { get; init; } = "";
    public string MessageId { get; init; } = "";
    public int State { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
    public bool StaticDialogFound { get; init; }
    public bool IsOpener { get; init; }
    public string RoomId { get; init; } = "";
    public string RoomCondition { get; init; } = "";
    public bool? RoomUnlocked { get; init; }
    public int? ConditionId { get; init; }
    public string ConditionType { get; init; } = "";
    public int TriggerCount { get; init; }
    public string ConditionStatus { get; init; } = "";
    public string StaticStatus { get; init; } = "";
}
