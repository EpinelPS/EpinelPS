using EpinelPS.Data;
using Google.Protobuf;

namespace EpinelPS.Models;

public class AccessToken
{
    public string Token { get; set; } = "";
    public long ExpirationTime { get; set; }
    public ulong UserID { get; set; }
}
public class FieldInfo
{
    public List<NetFieldStageData> CompletedStages { get; set; } = [];
    public List<NetFieldObject> CompletedObjects { get; set; } = [];
}

public class FieldInfoNew
{
    public List<int> CompletedStages { get; set; } = [];
    public List<NetFieldObject> CompletedObjects { get; set; } = [];
    public bool BossEntered { get; set; } = false;
}

public class CharacterModel
{
    public int Csn { get; set; } = 0;
    public int Tid { get; set; } = 0;
    public int CostumeId { get; set; } = 0;
    public int Level { get; set; } = 1;
    public int UltimateLevel { get; set; } = 1;
    public int Skill1Lvl { get; set; } = 1;
    public int Skill2Lvl { get; set; } = 1;
    public int Grade { get; set; } = 0;
    public bool IsMainForce { get; set; } = false;
}
public class MainQuestData
{
    public int TableId { get; set; } = 0;
    public bool IsReceieved { get; set; } = false;
}

public class UserPointData
{
    public int UserLevel { get; set; } = 1;
    public int ExperiencePoint { get; set; } = 0;
}

public class DbItemData
{
    public int ItemType { get; set; }
    public long Csn { get; set; }
    public int Count { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Position { get; set; }
    public int Corp { get; set; }
    public long Isn { get; set; }

    // For harmony cubes that can be equipped to multiple characters
    public List<long> CsnList { get; set; } = [];
}

public class EquipmentAwakeningData
{
    public long Isn { get; set; }
    public NetEquipmentAwakeningOption Option { get; set; }
    public bool IsNewData { get; set; }

    public EquipmentAwakeningData()
    {
        Option = new NetEquipmentAwakeningOption();
        IsNewData = false;
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
    public int CurrentSeason { get; set; }
    public int CurrentSubSeason { get; set; }
    public List<int> CurrentOptionList { get; set; } = [];
    public bool IsOverclock { get; set; } = false;
    public bool HasClearedLevel50 { get; set; } = false;
    public bool WasInfinitePopupChecked { get; set; } = false;
    public bool WasMainSeasonResetPopupChecked { get; set; } = true;
    public bool WasSubSeasonResetPopupChecked { get; set; } = true;
    public OverclockHighScoreData CurrentSeasonHighScore { get; set; }
    public OverclockHighScoreData CurrentSubSeasonHighScore { get; set; }
    public OverclockHighScoreData LatestOption { get; set; }
}
public class OverclockHighScoreData
{
    public int Season { get; set; }
    public int SubSeason { get; set; }
    public List<int> OptionList { get; set; } = [];
    public int OptionLevel { get; set; }
    public Google.Protobuf.WellKnownTypes.Timestamp? CreatedAt { get; set; }
}
public class SimRoomEvent
{
    public SimRoomEventLocationInfo Location { get; set; } = new();
    public bool Selected { get; set; }
    public SimRoomBattleEvent Battle { get; set; } = new();
    public SimRoomSelectionEvent Selection { get; set; } = new();
    public int EventCase { get; set; }
}
public class SimRoomEventLocationInfo
{
    public int Chapter { get; set; }
    public int Stage { get; set; }
    public int Order { get; set; }
}
public class SimRoomChapterInfo
{
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
    public long Csn { get; set; }
    public int Hp { get; set; }
}

public class ResetableData
{
    public int WipeoutCount { get; set; } = 0;
    public bool ClearedSimulationRoom { get; set; } = false;
    public int InterceptionTickets { get; set; } = 3;
    public List<int> CompletedDailyMissions { get; set; } = [];
    public int DailyMissionPoints { get; set; }
    public SimRoomData SimRoomData { get; set; } = new();

    public Dictionary<int, int> DailyCounselCount { get; set; } = [];

}
public class WeeklyResetableData
{
    public List<int> CompletedWeeklyMissions { get; set; } = [];
    public int WeeklyMissionPoints { get; set; }
}
public class OutpostBuffs
{
    public List<int> CreditPercentages { get; set; } = [];
    public List<int> CoreDustPercentages { get; set; } = [];
    public List<int> BattleDataPercentages { get; set; } = [];
    public List<int> UserExpPercentages { get; set; } = [];

    public List<int> GetPercentages(CurrencyType currency)
    {
        if (currency == CurrencyType.Gold)
            return CreditPercentages;
        else if (currency == CurrencyType.UserExp)
            return UserExpPercentages;
        else if (currency == CurrencyType.CharacterExp)
            return BattleDataPercentages;
        else if (currency == CurrencyType.CharacterExp2)
            return CoreDustPercentages;

        throw new InvalidOperationException();
    }
    public int GetTotalPercentages(CurrencyType currency)
    {
        int result = 0;
        var numbs = GetPercentages(currency);
        foreach (var item in numbs)
        {
            result += item;
        }

        return result;
    }
}

public class JukeBoxSetting
{
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

public class MogMinigameInfo
{
    public List<string> CompletedScenarios { get; set; } = [];
}
public class BadgeModel
{
    public string Location { get; set; } = "";
    public long Seq { get; set; }
    public BadgeContents BadgeContent { get; set; }
    public string BadgeGuid { get; set; } = "";

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
    public bool RecievedFinalReward { get; set; }
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