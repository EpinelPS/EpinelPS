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
        public int Diff = 0; // Default value for Diff
        public int LastStage = 0; // Default value for LastStage
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
    public class SimroomData
    {
        public int CurrentDifficulty;
        public int CurrentChapter;
        public bool Entered = false;
    }
    public class ResetableData
    {
        public int WipeoutCount = 0;
        public bool ClearedSimulationRoom = false;
        public int InterceptionTickets = 3;
        public List<int> CompletedDailyMissions = [];
        public int DailyMissionPoints;
        public SimroomData SimRoomData = new();

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
}