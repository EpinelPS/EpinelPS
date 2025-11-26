using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf;
using Paseto;
using Paseto.Builder;

namespace EpinelPS.Models
{
    public class AccessToken
    {
        public string Token = "";
        public long ExpirationTime;
        public ulong UserID;
    }
    public class FieldInfo
    {
        public List<NetFieldStageData> CompletedStages = [];
        public List<NetFieldObject> CompletedObjects = [];
    }

    public class FieldInfoNew
    {
        public List<int> CompletedStages = [];
        public List<NetFieldObject> CompletedObjects = [];
        public bool BossEntered = false;
    }

    public class CharacterModel
    {
        public int Csn = 0;
        public int Tid = 0;
        public int CostumeId = 0;
        public int Level = 1;
        public int UltimateLevel = 1;
        public int Skill1Lvl = 1;
        public int Skill2Lvl = 1;
        public int Grade = 0;
    }
    public class MainQuestData
    {
        public int TableId = 0;
        public bool IsReceieved = false;
    }

    public class UserPointData
    {
        public int UserLevel = 1;
        public int ExperiencePoint = 0;
    }

    public class ItemData
    {
        public int ItemType;
        public long Csn;
        public int Count;
        public int Level;
        public int Exp;
        public int Position;
        public int Corp;
        public long Isn;

        // For harmony cubes that can be equipped to multiple characters
        public List<long> CsnList = [];
    }

    public class EquipmentAwakeningData
    {
        public long Isn;
        public NetEquipmentAwakeningOption Option;
        public bool IsNewData;

        public EquipmentAwakeningData()
        {
            Option = new NetEquipmentAwakeningOption();
            IsNewData = false;
        }
    }
    public class EventData
    {
        public List<string> CompletedScenarios = [];
        public List<int> ClearedStages = []; // List of cleared stage IDs
        public int Diff = 0; // Default value for Diff
        public int LastStage = 0; // Default value for LastStage
    }
    public class LoginEventData
    {
        public List<int> Days = [];
        public int LastDay = 0; // Default value for LastDay
        public long LastDate = 0; // Default value for LastDate
    }

    public class SynchroSlot
    {
        /// <summary>
        /// Index of slot, 1 based
        /// </summary>
        public int Slot;
        /// <summary>
        /// Character CSN
        /// </summary>
        public long CharacterSerialNumber;
        /// <summary>
        /// Time when slot cooldown expires
        /// </summary>
        public long AvailableAt;
    }
    public class RecycleRoomResearchProgress
    {
        public int Level = 1;
        public int Exp;
        public int Attack;
        public int Defense;
        public int Hp;
    }

    // Simroom Data
    public class SimRoomData
    {
        public int CurrentDifficulty;
        public int CurrentChapter;
        public List<int> Buffs = [];
        public List<int> LegacyBuffs = [];
        public List<SimRoomEvent> Events = [];
        public List<SimRoomCharacterHp> RemainingHps = [];
        public List<SimRoomChapterInfo> ReceivedRewardChapters = [];
        public bool IsSimpleModeSkipEnabled = false;
        public bool Entered = false;
    }
    public class SimRoomEvent
    {
        public SimRoomEventLocationInfo Location = new();
        public bool Selected;
        public SimRoomBattleEvent Battle = new();
        public SimRoomSelectionEvent Selection = new();
        public int EventCase;
    }
    public class SimRoomEventLocationInfo
    {
        public int Chapter;
        public int Stage;
        public int Order;
    }
    public class SimRoomChapterInfo
    {
        public int Difficulty;
        public int Chapter;
    }
    public class SimRoomBattleEvent
    {
        public int Id;
        public List<int> BuffOptions = [];
        public int Progress;
        public int RemainingTargetHealth;
        public int BuffPreviewId;
    }
    public class SimRoomSelectionEvent
    {
        public int Id;
        public int SelectedNumber;
        public List<SimRoomSelectionGroupElement> Group = [];
    }
    public class SimRoomSelectionGroupElement
    {
        public int SelectionNumber;
        public int Id;
        public bool IsDone;
        public int RandomBuff;
    }
    public class SimRoomCharacterHp
    {
        public long Csn;
        public int Hp;
    }

    public class ResetableData
    {
        public int WipeoutCount = 0;
        public bool ClearedSimulationRoom = false;
        public int InterceptionTickets = 3;
        public List<int> CompletedDailyMissions = [];
        public int DailyMissionPoints;
        public SimRoomData SimRoomData = new();

        public Dictionary<int, int> DailyCounselCount = [];

    }
    public class WeeklyResetableData
    {
        public List<int> CompletedWeeklyMissions = [];
        public int WeeklyMissionPoints;
    }
    public class OutpostBuffs
    {
        public List<int> CreditPercentages = [];
        public List<int> CoreDustPercentages = [];
        public List<int> BattleDataPercentages = [];
        public List<int> UserExpPercentages = [];

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
        public NetJukeboxLocation Location;
        public NetJukeboxBgmType Type;
        public int TableId;
    }

    public class UnlockData
    {
        public bool ButtonAnimationPlayed = false;
        public bool PopupAnimationPlayed = false;

        public UnlockData() { }
        public UnlockData(bool button, bool popup)
        {
            ButtonAnimationPlayed = button;
            PopupAnimationPlayed = popup;
        }
    }

    public class MogMinigameInfo
    {
        public List<string> CompletedScenarios = [];
    }
    public class BadgeModel
    {
        public string Location = "";
        public long Seq;
        public BadgeContents BadgeContent;
        public string BadgeGuid = "";

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
        public Trigger Type;
        public long Id;
        public long CreatedAt;
        public int ConditionId;
        public int Value;

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
}