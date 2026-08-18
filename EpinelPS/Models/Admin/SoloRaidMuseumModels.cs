namespace EpinelPS.Models.Admin;

public sealed class SoloRaidMuseumRecordModel
{
    public ulong UserId { get; set; }
    public int StageId { get; set; }
    public bool NoLimit { get; set; }
    public int StageJoinCount { get; set; }
    public long TotalDamage { get; set; }
    public int TotalStep { get; set; }
    public long BestDamage { get; set; }
    public int BestStep { get; set; }
    public bool IsInProgress { get; set; }
    public double DebugDamageMultiplier { get; set; } = 1;
    public List<SoloRaidMuseumLogModel> Logs { get; set; } = [];
    public List<SoloRaidMuseumLogModel> CurrentLogs { get; set; } = [];
}

public sealed class SoloRaidMuseumLogModel
{
    public int TeamNumber { get; set; }
    public long Damage { get; set; }
    public int Step { get; set; }
}

public sealed class SoloRaidMuseumResetModel
{
    public ulong UserId { get; set; }
    public int StageId { get; set; }
}
