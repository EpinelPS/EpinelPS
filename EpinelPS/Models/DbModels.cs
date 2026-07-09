using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EpinelPS.Data;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class AccessToken
{
    public string Token { get; set; } = "";
    public long ExpirationTime { get; set; }
    public ulong UserID { get; set; }
}
public class ClearedTutorialData
{
    public int GroupId { get; set; }

    public int Id { get; set; }
    public int VersionGroup { get; set; }
}

public class CompletedFieldObject
{
    public int Id { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    public ulong UserId { get; set; }
    public DateTime ActionAt { get; set; }
    public int Type { get; set; }
    public string PositionId { get; set; }
    public string Json { get; set; }
}
public class FieldInfoNew
{
    public ulong Id { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    public ulong UserId { get; set; }
    public List<int> CompletedStages { get; set; } = [];
    public List<CompletedFieldObject> CompletedObjects { get; set; } = [];
    public List<int> FieldItemTableIdList { get; set; } = [];
    public List<int> AcquiredPasswordList { get; set; } = [];
    public List<int> UnlockedDoorList { get; set; } = [];
    public bool BossEntered { get; set; } = false;
    public string PositionJson { get; set; } = "";
    public string MapName { get; set; } = "";
}

public class CharacterModel
{
    [Key]
    public int Csn { get; set; } = 0;
    public int Tid { get; set; } = 0;
    public int CostumeId { get; set; } = 0;
    public int Level { get; set; } = 1;
    public int UltimateLevel { get; set; } = 1;
    public int Skill1Lvl { get; set; } = 1;
    public int Skill2Lvl { get; set; } = 1;
    public int Grade { get; set; } = 0;
    public bool IsMainForce { get; set; } = false;
    public int NameCode { get; set; }
    public int BondLevel { get; set; } = 1;
    public int BondLevelExp { get; set; }
    public bool Favorite { get; set; }
    public int TotalCounseledCount { get; set; }
    public List<int> CompletedDialogs { get; set; }
    public List<int> FlushableWatchedDialogIds { get; set; }
    public List<int> ObtainedRewardLevels { get; set; }
    public OriginalRareType RareType { get; set; }
    public CounselDialogCompleteRewardStatus RewardStatus { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    public ulong UserId { get; set; }
}
public class MainQuestData
{
    public int TableId { get; set; } = 0;
    public bool IsReceieved { get; set; } = false;
}

public class UserPointData
{
    [Obsolete]
    public int UserLevel { get; set; } = 1;
    [Obsolete]
    public int ExperiencePoint { get; set; } = 0;
}

public class DbItemData
{
    public int ItemType { get; set; }
    [Key]
    public long Csn { get; set; }
    public int Count { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Position { get; set; }
    public int Corp { get; set; }
    public long Isn { get; set; }

    // For harmony cubes that can be equipped to multiple characters
    public List<long> CsnList { get; set; } = [];
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    public ulong UserId { get; set; }
}

public class EquipmentAwakeningData
{
    [Key]
    public long Isn { get; set; }
    public bool IsNewData { get; set; }

    public int Option1Id { get; set; }
    public bool Option1Lock { get; set; }
    public bool IsOption1DisposableLock { get; set; }
    public int Option2Id { get; set; }
    public bool Option2Lock { get; set; }
    public bool IsOption2DisposableLock { get; set; }
    public int Option3Id { get; set; }
    public bool Option3Lock { get; set; }
    public bool IsOption3DisposableLock { get; set; }

    public EquipmentAwakeningData()
    {
        IsNewData = false;
    }
    public NetEquipmentAwakeningOption ToNet()
    {
        return new()
        {
            IsOption1DisposableLock = IsOption1DisposableLock,
            IsOption2DisposableLock = IsOption2DisposableLock,
            IsOption3DisposableLock = IsOption3DisposableLock,
            Option1Id = Option1Id,
            Option2Id = Option2Id,
            Option3Id = Option3Id,
            Option1Lock = Option1Lock,
            Option2Lock = Option2Lock,
            Option3Lock = Option3Lock
        };
    }
}
public class EventData
{
    public List<string> CompletedScenarios { get; set; } = [];
    public List<int> ClearedStages { get; set; } = []; // List of cleared stage IDs
    public int Diff { get; set; } = 0; // Default value for Diff
    public int LastStage { get; set; } = 0; // Default value for LastStage
    public int LastDay { get; set; } = 0;
    public int FreeTicket { get; set; } = 0;
}
public class LoginEventData
{
    public List<int> Days { get; set; } = [];
    public int LastDay { get; set; } = 0; // Default value for LastDay
    public long LastDate { get; set; } = 0; // Default value for LastDate
}
// EventMissionData
public class EventMissionData
{
    public List<int> MissionIdList { get; set; } = [];
    public List<int> DailyMissionIdList { get; set; } = [];
    public int LastDay { get; set; } = 0; // yyyyMMdd
    public long LastDate { get; set; } = 0; // Default value for LastDate
    public bool AllClear { get; set; } = false;// activity event has been completed
}
// EventShopBuyCountData
public class EventShopProductData
{
    public int ProductTid { get; set; } = 0;
    public int BuyCount { get; set; } = 0;
}

public class EventShopBuyCountData
{
    public int EventId { get; set; } = 0;
    public List<EventShopProductData> datas { get; set; } = [];
}

public class SynchroSlot
{
    public int Id { get; set; }
    /// <summary>
    /// Index of slot, 1 based
    /// </summary>
    public int Slot { get; set; }
    /// <summary>
    /// Character CSN
    /// </summary>
    public long CharacterSerialNumber { get; set; }
    /// <summary>
    /// Time when slot cooldown expires
    /// </summary>
    public long AvailableAt { get; set; }
}
public class RecycleRoomResearchProgress
{
    public int Level { get; set; } = 1;
    public int Exp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Hp { get; set; }
}

// Simroom Data
public class SimRoomData
{
    [Key]
    public int Id { get; set; }
    public int CurrentDifficulty { get; set; }
    public int CurrentChapter { get; set; }
    public List<int> Buffs { get; set; } = [];
    public List<int> LegacyBuffs { get; set; } = [];
    public List<SimRoomEvent> Events { get; set; } = [];
    public List<SimRoomCharacterHp> RemainingHps { get; set; } = [];
    public List<SimRoomChapterInfo> ReceivedRewardChapters { get; set; } = [];
    public bool IsSimpleModeSkipEnabled { get; set; } = false;
    public bool Entered { get; set; } = false;
    public OverclockData CurrentSeasonData { get; set; } = new();
}
public class OverclockData
{
    [Key] public ulong Id { get; set; }
    public int CurrentSeason { get; set; }
    public int CurrentSubSeason { get; set; }
    public List<int> CurrentOptionList { get; set; } = [];
    public bool IsOverclock { get; set; } = false;
    public bool HasClearedLevel50 { get; set; } = false;
    public bool WasInfinitePopupChecked { get; set; } = false;
    public bool WasMainSeasonResetPopupChecked { get; set; } = true;
    public bool WasSubSeasonResetPopupChecked { get; set; } = true;
    public OverclockHighScoreData CurrentSeasonHighScore { get; set; } = new();
    public OverclockHighScoreData CurrentSubSeasonHighScore { get; set; } = new();
    public OverclockHighScoreData LatestOption { get; set; } = new();
}
public class OverclockHighScoreData
{
    [Key] public ulong Id { get; set; }
    public int Season { get; set; }
    public int SubSeason { get; set; }
    public List<int> OptionList { get; set; } = [];
    public int OptionLevel { get; set; }
    public DateTime? CreatedAt { get; set; }
}
public class SimRoomEvent
{
    [Key]
    public int Id { get; set; }
    public SimRoomEventLocationInfo Location { get; set; } = new();
    public bool Selected { get; set; }
    public SimRoomBattleEvent Battle { get; set; } = new();
    public SimRoomSelectionEvent Selection { get; set; } = new();
    public int EventCase { get; set; }
}
public class SimRoomEventLocationInfo
{
    [Key]
    public int Id { get; set; }
    public int Chapter { get; set; }
    public int Stage { get; set; }
    public int Order { get; set; }
}
public class SimRoomChapterInfo
{
    [Key] public ulong Id { get; set; }
    public int Difficulty { get; set; }
    public int Chapter { get; set; }
}
public class SimRoomBattleEvent
{
    public int Id { get; set; }
    public List<int> BuffOptions { get; set; } = [];
    public int Progress { get; set; }
    public int RemainingTargetHealth { get; set; }
    public int BuffPreviewId { get; set; }
}
public class SimRoomSelectionEvent
{
    public int Id { get; set; }
    public int SelectedNumber { get; set; }
    public List<SimRoomSelectionGroupElement> Group { get; set; } = [];
}
public class SimRoomSelectionGroupElement
{
    public int SelectionNumber { get; set; }
    public int Id { get; set; }
    public bool IsDone { get; set; }
    public int RandomBuff { get; set; }
}
public class SimRoomCharacterHp
{
    [Key]
    public long Csn { get; set; }
    public int Hp { get; set; }
}

public class ResetableData
{
    [Key]
    public int Id;
    public int WipeoutCount { get; set; } = 0;
    public bool ClearedSimulationRoom { get; set; } = false;
    public int InterceptionTickets { get; set; } = 3;
    public List<int> CompletedDailyMissions { get; set; } = [];
    public int DailyMissionPoints { get; set; }
    public SimRoomData SimRoomData { get; set; } = new();
    public Dictionary<int, int> TowerCount { get; set; } = [];
    //public Dictionary<int, int> DailyCounselCount { get; set; } = [];
    public int DispatchCount { get; set; } = 0;
}

public class ResetableDataNew
{
    public int Id { get; set; }
    public int WipeoutCount { get; set; } = 0;
    public bool ClearedSimulationRoom { get; set; } = false;
    public int InterceptionTickets { get; set; } = 3;
    public List<int> CompletedDailyMissions { get; set; } = [];
    public int DailyMissionPoints { get; set; }
    public SimRoomData SimRoomData { get; set; } = new();
    public int[] TowerCount { get; set; } = [0, 0, 0, 0];
    //public Dictionary<int, int> DailyCounselCount { get; set; } = [];
    public int DispatchCount { get; set; } = 0;
    public List<DispatchData> Dispatches = new();
}
public class WeeklyResetableData
{
    public int Id { get; set; }
    public List<int> CompletedWeeklyMissions { get; set; } = [];
    public int WeeklyMissionPoints { get; set; }
}

public class JukeBoxSetting
{
    [Key]
    public long Id { get; set; }
    public NetJukeboxLocation Location { get; set; }
    public NetJukeboxBgmType Type { get; set; }
    public int TableId { get; set; }
}

public class UnlockData
{
    public bool ButtonAnimationPlayed { get; set; } = false;
    public bool PopupAnimationPlayed { get; set; } = false;

    public UnlockData() { }
    public UnlockData(bool button, bool popup)
    {
        ButtonAnimationPlayed = button;
        PopupAnimationPlayed = popup;
    }
}

public class BadgeModel
{
    public string Location { get; set; } = "";
    [Key]
    public long Seq { get; set; }
    public BadgeContents BadgeContent { get; set; }
    public string BadgeGuid { get; set; } = "";
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    public ulong UserId { get; set; }

    public BadgeModel() { }
    public BadgeModel(NetBadge badge)
    {
        Location = badge.Location;
        Seq = badge.Seq;
        BadgeContent = badge.BadgeContent;
        BadgeGuid = new Guid([.. badge.BadgeGuid]).ToString();
    }

    public NetBadge ToNet()
    {
        return new NetBadge()
        {
            BadgeContent = BadgeContent,
            BadgeGuid = ByteString.CopyFrom(new Guid(BadgeGuid).ToByteArray()),
            Location = Location,
            Seq = Seq
        };
    }
}

public class TriggerModel
{
    public Trigger Type { get; set; }
    public long Id { get; set; }
    public long CreatedAt { get; set; }
    public int ConditionId { get; set; }
    public int Value { get; set; }

    public NetTrigger ToNet()
    {
        return new()
        {
            ConditionId = ConditionId,
            CreatedAt = CreatedAt,
            Seq = Id,
            Trigger = (int)Type,
            UserValue = Value
        };
    }
}
public class ConversationChoice
{

}
public class ConversationMessage
{
    public string ConversationId { get; set; } = "";
    public long CreatedAt { get; set; }
    public ulong Seq { get; set; }
    public string Id { get; set; } = "";
    public int State { get; set; }
}
public class LostSectorData
{
    public bool IsOpen { get; set; }
    public bool IsPlaying { get; set; }
    public string Json { get; set; } = "";
    public Dictionary<string, NetLostSectorFieldObject> Objects { get; set; } = [];
    public Dictionary<string, int> ClearedStages { get; set; } = [];
    public List<NetLostSectorTeamPosition> TeamPositions { get; set; } = [];
    public int ObtainedRewards { get; set; } = 0;
    public bool ReceivedFinalReward { get; set; }
    public bool CompletedPerfectly { get; set; }
}

public class PassRankData
{
    public int PassRank { get; set; } = 0;
    public bool IsNormalRewarded { get; set; } = false;
    public bool IsPremiumRewarded { get; set; } = false;
}
public class PassMissionData
{
    public int PassMissionId { get; set; } = 0;
    public bool IsComplete { get; set; } = false;
}
public class PassData
{
    public int PassId { get; set; } = 0;
    public int PassPoint { get; set; } = 0;
    public int PassSkipCount { get; set; } = 0;
    public bool PremiumActive { get; set; } = false;
    public string LastCompleteAt { get; set; } = "";
    public List<PassRankData> PassRankList { get; set; } = [];
    public List<PassMissionData> PassMissionList { get; set; } = [];
}

// MiniGameAzxData
public class MiniGameAzxData
{
    public List<MiniGameAzxScoreData> ScoreDatas { get; set; } = [];
    public List<AchievementMissionData> AchievementMissionDataList { get; set; } = [];
    public List<ConditionalBoardData> ConditionalBoardDataList { get; set; } = [];
    public List<ConditionalCharacterData> ConditionalCharacterDataList { get; set; } = [];
    public List<ConditionalSkillData> ConditionalSkillDataList { get; set; } = [];
    public List<CutSceneData> CutSceneDataList { get; set; } = [];
    public bool IsTutorialConfirmed { get; set; } = false;
    public int SelectedBoardId { get; set; }
    public int SelectedCharacterId { get; set; }
    public Dictionary<int, int> SkillCount { get; set; } = [];
    public Dictionary<int, int> CharacterCount { get; set; } = [];
}
public class MiniGameAzxScoreData
{
    public int AzxId { get; set; }
    public int DateDay { get; set; }
    public int AccumulatedScore { get; set; }
    public int HighScore { get; set; }
    public bool IsDailyRewarded { get; set; } = false;
    public Google.Protobuf.WellKnownTypes.Duration? HighScoreTime { get; set; }
}
public class AchievementMissionData
{
    public int MissionId { get; set; }
    public bool IsReceived { get; set; }

}
public class ConditionalBoardData
{
    public int BoardId { get; set; }
    public bool IsUnlocked { get; set; }
}
public class ConditionalCharacterData
{
    public int CharacterId { get; set; }
    public bool IsUnlocked { get; set; }
}
public class ConditionalSkillData
{
    public int SkillId { get; set; }
    public bool IsUnlocked { get; set; }
}
public class CutSceneData
{
    public int CutSceneId { get; set; }
    public bool IsNew { get; set; }
}

// Solo Raid Data
public class SoloRaidInfo
{
    public int RaidId { get; set; }
    public int RaidOpenCount { get; set; }
    public int TrialCount { get; set; }
    public int LastDateDay { get; set; }
    public List<SoloRaidLevelData> SoloRaidLevels { get; set; } = []; // key: raidLevel
}
public class SoloRaidLevelData
{
    public int RaidLevel { get; set; }
    public int RaidJoinCount { get; set; }
    public long Hp { get; set; }
    public long TotalDamage { get; set; }
    public bool IsClear { get; set; }
    public SoloRaidStatus Status { get; set; }
    public SoloRaidType Type { get; set; }
    public bool IsOpen { get; set; }
    public List<SoloRaidLogData> Logs { get; set; } = [];
}
public class SoloRaidLogData
{
    public long Damage { get; set; }
    public bool Kill { get; set; }
    public List<TeamCharacterData> Team { get; set; } = [];

    public NetSoloRaidLog ToNet()
    {
        return new NetSoloRaidLog()
        {
            Damage = Damage,
            Kill = Kill,
            Team = { Team.Select(x => x.ToNet()).ToList() },
        };
    }
}
public class TeamCharacterData
{
    public int Slot { get; set; }
    public long Csn { get; set; }
    public int Tid { get; set; }
    public int Lv { get; set; }
    public int Combat { get; set; }
    public int CostumeId { get; set; }

    public NetSoloRaidTeamCharacter ToNet()
    {
        return new NetSoloRaidTeamCharacter()
        {
            Slot = Slot,
            Tid = Tid,
            Lv = Lv,
            Combat = Combat,
            CostumeId = CostumeId,
        };
    }
}

public class MiniGameStoryChoice
{
    public int Id { get; set; }
    public List<string> Choices { get; set; } = [];
    public List<string> SeenChoices { get; set; } = [];
    public string LastSeen { get; set; }
}


public class MiniGameScenarios
{
    public int ArcadeId { get; set; }
    public List<string> CompletedScenarios { get; set; } = [];

}

public class DispatchData
{
    public ulong Id { get; set; }
    public int TableId { get; set; }
    public bool Running { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public NetUserDispatchData ToNet()
    {
        return new()
        {
            StartAt = StartAt.Ticks,
            EndAt = EndAt.Ticks,
            IsRun = Running == true ? 1 : 0,
            Tid = TableId
        };
    }
}

public class DispatchDataSelectable
{
    public ulong Id { get; set; }
    public int DispatchGroupId { get; set; }
    public int SelectSlotId { get; set; }
    public int SelectTid { get; set; }
    public bool Running { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public NetSelectableDispatchData ToNet()
    {
        return new()
        {
            StartAt = StartAt.Ticks,
            EndAt = EndAt.Ticks,
            IsRun = Running,
            DispatchGroupId = DispatchGroupId,
            SelectSlotId = SelectSlotId,
            SelectTid = SelectTid
        };
    }
}

public class StellarBladeDatas
{
    public List<NetStellarBladeCurrency> Currency { get; set; } = [];
    public NetStellarBladeCharacterData CharacterData { get; set; } = new();
    public List<NetStellarBladeMissionData> MissionData { get; set; } = [];
    public List<NetStellarBladeMissionData> DailyMissionData { get; set; } = [];
    public List<NetStellarBladeMissionData> DailyPointMissionData { get; set; } = [];
    public Dictionary<int, ResArcadeGetStellarBladeStatistics.Types.NetStatisticsData> StatisticsData { get; set; } = [];
    public Dictionary<int, SBStageDatas> BestStageDatas { get; set; } = [];
    public List<int> TutorialList { get; set; } = [];
    public int LastEnteredStageId { get; set; }
    public List<int> SbItemIdList { get; set; } = [];
    public int DailyPoint { get; set; } = 0;
    public int Today { get; set; }
}

public class SBStageDatas
{
    public int BestDealtDamage { get; set; }
    public int BestScore { get; set; }
    public Duration BestDuration { get; set; }

}


public class TtsDatas
{
    public Dictionary<MiniGameTtsDifficulty, Dictionary<int, NetMiniGameTtsBadgeData>> BadgeData { get; set; } = [];
    public Dictionary<int, NetMiniGameTtsMissionData> MissionData { get; set; } = [];
    public List<NetMiniGameTtsScoreData> ScoreData { get; set; } = [];
    public List<NetMiniGameTtsSongPlayCount> SongPlayCount { get; set; } = [];
    public List<NetMiniGameTtsSongPlayData> SongPlayData { get; set; } = [];
    public NetUserMiniGameTtsSkinData SkinData { get; set; } = new();
    public List<int> BuySkinObject { get; set; } = [];

    public List<int> MissionCompleteList { get; set; } = [];

    public bool IsFinishTutorial { get; set; } = false;

    public MiniGameTtsDifficulty LastDifficulty = MiniGameTtsDifficulty.Normal;

    /// <summary>
    /// 徽章歌曲ID
    /// </summary>
    public List<int> BadgeSongId { get; set; } = [];

    /// <summary>
    /// 解锁歌曲ID
    /// </summary>
    public List<int> UnlockSongId { get; set; } = [];

    public int AllPlayCount { get; set; } = 0;

    public long TotalScore { get; set; } = 0;

    /// <summary>
    /// 购买的AlbumIds
    /// </summary>
    public List<int> PurchasedAlbumIds { get; set; } = [];

    public Duration TotalPlayTime { get; set; } = new();

    public Timestamp NewProductPopUp { get; set; } = new();

    public Timestamp DateFromShop { get; set; } = new();
}



public class SongRankKey
{
    public int SongId { get; set; }
    public MiniGameTtsRankingType RankType { get; set; }   // Server, Friend, Guild

}


public class TowerDefenseData
{
    public int ChallengeMaxScore { get; set; } = 0;
    public List<int> ClearedStageIdList { get; set; } = [];
    public List<int> ClearedTutorialIdList { get; set; } = [];
    public List<NetArcadeTowerDefenseMissionProgress> MissionProgressList { get; set; } = [];
    public int UpgradeCurrency { get; set; } = 0;
    public List<int> UpgradeIdList { get; set; } = [];
    public int LastEnteredStageId { get; set; } = 0;
}

public class SongRankData
{

    public long UserId { get; set; }               // 用户ID（新增）
    public int SongId { get; set; }
    public MiniGameTtsRankingType RankType { get; set; }
    public MiniGameTtsDifficulty Difficulty { get; set; }
    public int Score { get; set; }
    public long UpdateTime { get; set; }  // Unix timestamp
    public int IsDeleted { get; set; }
}

public class MiniGameTtsTotalRankRecord
{
    public long UserId { get; set; }
    public long Score { get; set; }
    public MiniGameTtsRankingType RankType { get; set; }
    public long UpdatedAt { get; set; }
}

public class SqlSongRankKey
{
    public int UserId { get; set; }               // 用户ID（新增）
    public int SongId { get; set; }
    public MiniGameTtsRankingType RankType { get; set; }
    public MiniGameTtsDifficulty Difficulty { get; set; }
}
public class MailAttachment
{
    public int Type { get; set; }   // RewardType
    public int Id { get; set; }      // RewardId
    public int Count { get; set; }   // RewardValue
}

public class ArcadeScoreRecord
{
    public int Id { get; set; }
    public long GuildId { get; set; }
    public ulong UserId { get; set; }
    public long Score { get; set; }
    public int ArcadeId { get; set; }
    public int ModeId { get; set; }
}

public class ArcadeBBQData
{
    public int Id { get; set; }
    public int ArcadeId { get; set; }
    public int HighScore { get; set; }
    public long TotalScore { get; set; }
    public List<int> StepUpRewarded { get; set; }
    public List<int> RecordedCutScenes { get; set; }
    public int PlayCount { get; set; }

    public NetArcadeBBQData ToNet()
    {
        NetArcadeBBQData result = new()
        {
            ArcadeId = ArcadeId,
            HighScore = HighScore,
            PlayCount = PlayCount,
            TotalAccumulatedScore = TotalScore
        };

        result.StepUpRewardedList.AddRange(StepUpRewarded);
        result.RecordedCutSceneList.AddRange(RecordedCutScenes);

        return result;
    }
}

public class DbMessage
{
    public long Id { get; set; }
    public string ConversationId { get; set; }
    public string MessageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int State { get; set; }

    public NetMessage ToNet()
    {
        return new()
        {
            ConversationId = ConversationId,
            CreatedAt = CreatedAt.Ticks,
            MessageId = MessageId,
            State = State,
            Seq = Id
        };
    }
}