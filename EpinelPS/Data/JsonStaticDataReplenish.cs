using MemoryPack;

namespace EpinelPS.Data;

[MemoryPackable]
public partial class SimulationRoomStageLocationRecord
{
    public int Id;
    public int ScheduleGroupId;
    public int ChapterId;
    public int StageGroupId;
    public int StageEssentialValue;
    public int EventSelectionValue;
    public SimulationRoomLocation Location;
    public int Weight;
}

[MemoryPackable]
public partial class SimulationRoomBattleEventRecord
{
    public int Id { get; set; }
    public SimulationRoomEvent EventType { get; set; }
    public SimulationRoomScalingType ScalingType { get; set; }
    public SimulationRoomConditionType DifficultyConditionType { get; set; }
    public int DifficultyConditionValue { get; set; }
    public SimulationRoomConditionType ChapterConditionType { get; set; }
    public int ChapterConditionValue { get; set; }
    public int Weight { get; set; }
    public bool SpotAutocontrol { get; set; }
    public int MonsterStageLv { get; set; }
    public int DynamicObjectStageLv { get; set; }
    public int StandardBattlePower { get; set; }
    public int StageStatIncreaseGroupId { get; set; }
    public bool IsUseQuickBattle { get; set; }
    public int SpotId { get; set; }
    public bool UseOcMode { get; set; }
    public int UseSeasonId { get; set; }
    public int BattleEventGroup { get; set; }
}

[MemoryPackable]
public partial class SimulationRoomBuffPreviewRecord
{
    public int Id { get; set; }
    public SimulationRoomEvent EventType { get; set; }
    public PreviewType PreviewType { get; set; }
    public string? PreviewTarget { get; set; }
    public int Weight { get; set; }
    public string? DescriptionLocalkey { get; set; }
}

[MemoryPackable]
public partial class SimulationRoomBuffRecord
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public SimulationRoomBuffMainTarget MainTarget { get; set; }
    public List<SimulationRoomBuffSubTarget>? SubTarget { get; set; }
    public SimulationRoomBuffGrade Grade { get; set; }
    public int Weight { get; set; }
    public SimulationRoomBubbleType BubbleType { get; set; }
    public string? NameLocalkey { get; set; }
    public string? DescriptionLocalkey { get; set; }
    public List<string>? ParameterLocalkey { get; set; }
    public string? ResourceId { get; set; }
    public SimulationRoomBuffFunctionType FunctionType { get; set; }
    public List<SimulationRoomBuffValueData>? BuffValue { get; set; }
}

[MemoryPackable]
public partial class SimulationRoomSelectionEventRecord
{
    public int Id { get; set; }
    public SimulationRoomEvent EventType { get; set; }
    public SimulationRoomConditionType ChapterConditionType { get; set; }
    public int ChapterConditionValue { get; set; }
    public int SelectionGroupId { get; set; }
    public int SelectionValue { get; set; }
    public string? NameLocalkey { get; set; }
    public string? DescriptionLocalkey { get; set; }
    public int Weight { get; set; }
}

[MemoryPackable]
public partial class SimulationRoomBuffValueData
{
    public int FunctionValueLevel { get; set; }
    public int BattlePowerLevel { get; set; }
}

public enum SimulationRoomScalingType
{
    DataRef = 0,
    None = 1,
}

public enum SimulationRoomConditionType
{
    Range = 0, // 0x0
    Select = 1, // 0x0
}

public enum PreviewType
{
    MainTarget = 0, // 0x0
    Bubble = 1, // 0x0
    Grade = 2
}

public enum SimulationRoomBuffMainTarget
{
    Shoot = 0, // 0x0
    Attack = 1, // 0x0
    Survive = 2
}

public enum SimulationRoomBuffSubTarget
{
    AR = 0,
    RL = 1,
    SR = 2,
    MG = 3,
    SG = 4,
    SMG = 5,
    ELYSION = 6,
    MISSILIS = 7,
    TETRA = 8,
    PILGRIM = 9,
    Attacker = 10,
    Defender = 11,
    Supporter = 12,
    ALL = 13
}

public enum SimulationRoomBuffGrade
{
    R = 0,
    SR = 1,
    SSR = 2,
    EPIC = 3
}

public enum SimulationRoomBubbleType
{
    TypeA = 0,
    TypeB = 1,
    TypeC = 2,
    TypeD = 3
}

public enum SimulationRoomBuffFunctionType
{
    Function = 0,
    HealAfterBattle = 1
}