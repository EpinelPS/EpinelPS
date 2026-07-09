
using System.ComponentModel.DataAnnotations.Schema;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class User
{
    // General
    public ulong ID { get; set; }
    public string Nickname { get; set; } = "Player";
    public bool IsBanned { get; set; } = false;
    public DateTime BanStart { get; set; }

    public int BanId { get; set; } = 0;
    public DateTime BanEnd { get; set; }
    public ICollection<TriggerModelNew> Triggers { get; set; } = new List<TriggerModelNew>();
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public DateTime BattleTime { get; set; } = DateTime.UtcNow;
    
    public Dictionary<int, ClearedTutorialData> ClearedTutorialData = [];

    // Levels
    public int UserLevel { get; set; } = 1;
    public int ExperiencePoint { get; set; } = 0;

    // Campaign
    public int LastNormalStageCleared { get; set; }
    public int LastStoryStageCleared { get; set; }
    public int LastHardStageCleared { get; set; }
    public int LastClearedDifficulty { get; set; }
    public List<FieldInfoNew> FieldInfo { get; set; } = [];
    public List<string> CompletedScenarios { get; set; } = [];

    // Profile appearance
    public int ProfileIconId { get; set; } = 39900;
    public bool ProfileIconIsPrism { get; set; } = false;
    public int ProfileFrame { get; set; } = 25;

    public int TitleId { get; set; } = 1;

    // Gacha
    public bool sickpulls { get; set; } = false;

    public int GachaTutorialPlayCount { get; set; } = 0;
    
    // Resetable data
    public DateTime LastReset { get; set; } = DateTime.MinValue;
    public DateTime LastWeeklyReset { get; set; } = DateTime.MinValue;

    // Outpost - Synchro device
    public bool SynchroDeviceUpgraded { get; set; } = false;
    public int SynchroDeviceLevel { get; set; } = 200;

    // Outpost - level
    public int InfraCoreExp { get; set; } = 0;
    public int InfraCoreLvl { get; set; } = 1;

    // Outpost - dispatch

    public int DispatchLv { get; set; } = 1;
    public int DispatchCollectionLv { get; set; } = 0;
    public int DispatchFavoriteLv { get; set; } = 0;
    public int DispatchResetCount { get; set; } = 0;

    // Characters
    public List<CharacterModel> Characters { get; set; } = [];
    // Teams
    public long[] RepresentationTeamDataNew { get; set; } = [];

    // Items
    public List<DbItemData> Items { get; set; } = [];


    public List<DbMessage> MessengerData { get; set; } = [];

    public List<int> CompletedAchievements { get; set; } = [];

    // TODO ORGANIZE



    public Dictionary<CurrencyType, long> Currency  = new() {
            { CurrencyType.ContentStamina, 2 }
        };
    public List<SynchroSlot> SynchroSlots { get; set; } = [];
    public Dictionary<int, RecycleRoomResearchProgress> ResearchProgress  = [];

    public ResetableDataNew ResetableData { get; set; } = new();
    public WeeklyResetableData WeeklyResetableData { get; set; } = new();
    public List<EquipmentAwakeningData> EquipmentAwakenings { get; set; } = [];
    // Outpost buildings data
    public List<NetUserOutpostData> OutpostBuildings = [];// { get; set; } = [];

    public NetWallpaperData[] WallpaperList { get; set; } = [];
    public NetWallpaperBackground[] WallpaperBackground = []; // { get; set; } = [];
    public NetWallpaperJukeboxFavorite[] WallpaperFavoriteList = [];// { get; set; } = [];
    public NetWallpaperPlaylist[] WallpaperPlaylistList = [];// { get; set; } = [];
    public NetWallpaperJukebox[] WallpaperJukeboxList = [];// { get; set; } = [];
    public List<int> LobbyDecoBackgroundList { get; set; } = [];
    public List<int> LiveWallpaperList { get; set; } = [];

    public List<int> CostumeList { get; set; } = [];

    public List<int> JukeboxThemeList { get; set; } = [];

    public List<int> StickerList { get; set; } = [];
    public List<int> BackgroundList { get; set; } = [];
    public ProfileCardDecorationLayout DecorationLayout = new(); // { get; set; } = new();
    public List<int> IconList { get; set; } = [];
    public List<int> FrameList { get; set; } = [];
    public List<int> TitleList { get; set; } = [];
    public Dictionary<long, NetUserMailData> MailDatas  = [];       

    public Dictionary<int, NetUserTeamData> UserTeams = [];
    public Dictionary<int, bool> MainQuestData  = [];
    public Dictionary<int, bool> SubQuestData  = [];
    public Dictionary<int, bool> InfraCoreRewardReceived  = [];

    public int OutpostBattleLevel { get; set; } = 1;
    public int OutpostBattleLevelExp { get; set; } = 0;
    public List<int> CompletedTacticAcademyLessons { get; set; } = [];
    public List<int> CompletedSideStoryStages { get; set; } = [];
    public List<int> ViewedSideStoryStages { get; set; } = [];
    public List<int> ClearedOutpostScenarioIds { get; set; } = [];

    public List<int> Memorial { get; set; } = [];
    public List<int> JukeboxBgm { get; set; } = [];
    public List<NetUserFavoriteItemData> FavoriteItems = []; // TODO

    public List<NetUserFavoriteItemQuestData> FavoriteItemQuests = []; // TODO
    public Dictionary<int, int> TowerProgress  = [];    

    public JukeBoxSetting LobbyMusic = new();// { get; set; } = new() { Location = NetJukeboxLocation.Lobby, TableId = 2, Type = NetJukeboxBgmType.JukeboxTableId };
    public JukeBoxSetting CommanderMusic = new();// { get; set; } = new() { Location = NetJukeboxLocation.CommanderRoom, TableId = 5, Type = NetJukeboxBgmType.JukeboxTableId };
    public List<int> CreditPercentages { get; set; } = [];
    public List<int> CoreDustPercentages { get; set; } = [];
    public List<int> BattleDataPercentages { get; set; } = [];
    public List<int> UserExpPercentages { get; set; } = [];

    public Dictionary<int, UnlockData> ContentsOpenUnlocked = [];

    public List<BadgeModel> Badges { get; set; } = [];



    public Dictionary<int, LostSectorData> LostSectorData  = [];

    // Event data
    public Dictionary<int, EventData> EventInfo  = [];
    public Dictionary<int, LoginEventData> LoginEventInfo= [];
    public Dictionary<int, EventMissionData> EventMissionInfo = []; // key: eventId
    public Dictionary<int, EventShopBuyCountData> EventShopBuyCountInfo  = []; // key: eventId


    public List<NetPlaySodaEachGameInfo> ArcadePlaySodaInfoList = [];
   // public NetArcadeMvgData ArcadeInTheMirrorData { get; set; } = new();

    public Dictionary<int, PassData> UserPassInfo = []; // user pass data, key is PassId

    public List<int> LobbyPrivateBannerIds = [];
    
    // solo raid data
    public Dictionary<int, SoloRaidInfo> SoloRaidData = []; // key: raidId


    // Outpost dispatch
    public List<int> DispatchClearList { get; set; } = [];
    public List<DispatchDataSelectable> SelectableDispatchData { get; set; } = [];

    public int? guildId { get; set; }
    public long? LeaveAt { get; set; }


    // Minigame data
    public Dictionary<int,MiniGameScenarios> MiniGameScenarios = new();
    public Dictionary<int, MiniGameAzxData> MiniGameAzxInfo  = [];
    public Dictionary<int, MiniGameStoryChoice> MiniGameStoryChoice  = [];
    public ArcadeBBQData BBQInfoData  = new();//{ get; set; } = new();

    public Dictionary<int, TtsDatas> TTSGameData  = new();
    public Dictionary<int, StellarBladeDatas> StellarBladeDatas  = new();
    public Dictionary<int, TowerDefenseData> TowerDefenseDatas  = new();

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


    public TriggerModelNew AddTrigger(Trigger type, int value, int conditionId = 0)
    {
        TriggerModelNew t = new()
        {
            Type = type,
            ConditionId = conditionId,
            CreatedAt = DateTime.UtcNow.AddHours(9).Ticks,
            Value = value
        };

        Triggers.Add(t);
        GameContext.Instance.SaveChanges();

        return t;
    }

    public void AddBadge(BadgeContents type, string location)
    {
        var badge = new BadgeModel()
        {
            BadgeContent = type,
            Location = location,
            BadgeGuid = Guid.NewGuid().ToString()
        };

        Badges.Add(badge);
    }


    public void AddUnique<T>(List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    public void SetQuest(int tId, bool ReceivedReward)
    {
        if (!MainQuestData.TryAdd(tId, ReceivedReward))
        {
            MainQuestData[tId] = ReceivedReward;
            return;
        }
    }

    public void SetSubQuest(int tId, bool ReceivedReward)
    {
        if (!SubQuestData.TryAdd(tId, ReceivedReward))
        {
            SubQuestData[tId] = ReceivedReward;
            return;
        }
    }

    public int GenerateUniqueItemId()
    {
        var num = Rng.RandomId();

        while (Items.Any(x => x.Isn == num))
        {
            num = Rng.RandomId();
        }

        return num;
    }
    public int GenerateUniqueCharacterId()
    {
        var num = Rng.RandomId();

        while (Characters.Any(x => x.Csn == num))
        {
            num = Rng.RandomId();
        }

        return num;
    }
    public bool IsStageCompleted(int Id)
    {
        foreach (var item in FieldInfo)
        {
            if (item.CompletedStages.Contains(Id))
            {
                return true;
            }
        }

        return false;
    }

    public long GetCurrencyVal(CurrencyType type)
    {
        if (Currency.TryGetValue(type, out long value))
            return value;
        else
        {
            Currency.Add(type, 0);
            return 0;
        }
    }
    public void AddCurrency(CurrencyType type, long val)
    {
        if (!Currency.TryAdd(type, val)) Currency[type] += val;
    }
    public bool SubtractCurrency(CurrencyType type, long val)
    {
        if (type == CurrencyType.FreeCash)
        {
            if (Currency.ContainsKey(type))
            {
                if (Currency[type] < val)
                {
                    long diff = val - Currency[type];
                    if (Currency.ContainsKey(CurrencyType.ChargeCash))
                    {
                        if (Currency[CurrencyType.ChargeCash] > diff)
                        {
                            Currency[type] = 0;
                            Currency[CurrencyType.ChargeCash] -= diff;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        if (Currency.ContainsKey(type)) Currency[type] -= val;
        else return false;

        if (Currency[type] < 0)
        {
            Currency[type] += val;
            return false;
        }
        return true;
    }
    public bool CanSubtractCurrency(CurrencyType type, long val)
    {
        if (Currency.ContainsKey(type))
        {
            if (Currency[type] >= val) return true;
            else return false;
        }
        else
        {
            if (val == 0) return true;
            else return false;
        }
    }

    public bool HasCharacter(int c)
    {
        // Step 1: Get the 'NameCode' of the input character with Tid 'c'
        if (GameData.Instance.CharacterTable.TryGetValue(c, out var inputCharacterRecord))
        {
            int targetNameCode = inputCharacterRecord.NameCode;
            // Step 2: Find all character IDs in 'characterTable' that have the same 'NameCode'
            var matchingCharacterIds = GameData.Instance.CharacterTable.Where(kvp => kvp.Value.NameCode == targetNameCode).Select(kvp => kvp.Key).ToHashSet();

            // Step 3: Check if any of your owned characters have a 'Tid' in the set of matching IDs
            return Characters.Any(ownedCharacter => matchingCharacterIds.Contains(ownedCharacter.Tid));

        }
        else
        {   // The character with Tid 'c' does not exist in 'characterTable'
            return false;
        }
    }

    public CharacterModel? GetCharacter(int c)
    {
        // Step 1: Get the 'NameCode' of the input character with Tid 'c'
        if (GameData.Instance.CharacterTable.TryGetValue(c, out var inputCharacterRecord))
        {
            int targetNameCode = inputCharacterRecord.NameCode;
            // Step 2: Find all character IDs in 'characterTable' that have the same 'NameCode'
            var matchingCharacterIds = GameData.Instance.CharacterTable.Where(kvp => kvp.Value.NameCode == targetNameCode).Select(kvp => kvp.Key).ToHashSet();

            // Step 3: Check if any of your owned characters have a 'Tid' in the set of matching IDs
            return Characters.Where(ownedCharacter => matchingCharacterIds.Contains(ownedCharacter.Tid)).FirstOrDefault();

        }
        else
        {   // The character with Tid 'c' does not exist in 'characterTable'
            return null;
        }
    }

    public CharacterModel? GetCharacterBySerialNumber(long value)
    {
        if (value == 0) return null;
        return Characters.Where(x => x.Csn == value).FirstOrDefault();
    }

    internal bool GetSynchro(long csn)
    {
        return SynchroSlots.Where(x => x.CharacterSerialNumber == csn).Any();
    }
    internal int GetCharacterLevel(int csn)
    {
        var c = GetCharacterBySerialNumber(csn) ?? throw new Exception("failed to lookup character");
        return GetCharacterLevel(csn, c.Level);
    }
    internal int GetCharacterLevel(int csn, int characterLevel)
    {
        foreach (var item in SynchroSlots)
        {
            if (item.CharacterSerialNumber == csn)
            {
                return GetSynchroLevel();
            }
        }
        return characterLevel;
    }
    internal int GetSynchroLevel()
    {
        if (SynchroDeviceUpgraded)
            return SynchroDeviceLevel;
        var highestLevelCharacters = Characters.OrderByDescending(x => x.Level).Take(5).ToList();


        if (highestLevelCharacters.Count > 0)
        {
            return highestLevelCharacters.Last().Level;
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    /// Removes the specified amount of items by their ID. Returns the amount of items removed.
    /// </summary>
    /// <param name="isn"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int RemoveItemBySerialNumber(long isn, int count)
    {
        int removed = 0;
        foreach (var item in Items.ToList())
        {
            if (count == 0)
                break;

            if (item.Isn == isn)
            {
                removed++;
                item.Count -= count;

                if (item.Count < 0)
                {
                    item.Count = 0;
                }
            }
        }

        return removed;
    }

    public DbMessage CreateMessage(MessengerDialogRecord r, int state = 0)
    {
        return CreateMessage(r.ConversationId, r.Id, state);
    }

    public DbMessage CreateMessage(string conversationId, string messageId, int state = 0)
    {
        var msg = new DbMessage()
        {
            ConversationId = conversationId,
            CreatedAt = DateTime.UtcNow,
            MessageId = messageId,
            State = state
        };
        MessengerData.Add(msg);
        GameContext.Instance.SaveChanges(); // populate ID
        return msg;
    }

    private bool ShouldResetUser()
    {
        var nowLocal = DateTime.UtcNow;

        // Compute the last reset threshold (most recent 2 PM before or at nowLocal)
        DateTime todayResetTime = new(
            nowLocal.Year,
            nowLocal.Month,
            nowLocal.Day,
            JsonDb.Instance.ResetHourUtcTime, 0, 0
        );

        if (nowLocal < todayResetTime)
        {
            todayResetTime = todayResetTime.AddDays(-1);
        }

        // If user's last reset was before the last scheduled 2 PM, they need reset
        return LastReset < todayResetTime;
    }

    private bool ShouldResetWeekly()
    {
        var nowLocal = DateTime.UtcNow;

        // Calculate the most recent Tuesday reset time
        DayOfWeek currentDay = nowLocal.DayOfWeek;
        int daysSinceTuesday = ((int)currentDay - (int)DayOfWeek.Tuesday + 7) % 7;

        // Get the date of the most recent Tuesday
        DateTime thisTuesday = nowLocal.Date.AddDays(-daysSinceTuesday);

        // Compute the weekly reset time
        DateTime weeklyResetTime = new(
            thisTuesday.Year,
            thisTuesday.Month,
            thisTuesday.Day,
            JsonDb.Instance.ResetHourUtcTime, 0, 0
        );

        // If nowLocal is before the weekly reset time, subtract a week
        if (nowLocal < weeklyResetTime)
        {
            weeklyResetTime = weeklyResetTime.AddDays(-7);
        }

        // If user's last reset was before the last scheduled 2 PM, they need reset
        return LastWeeklyReset < weeklyResetTime;
    }

    public void ResetDataIfNeeded()
    {
        bool needsSave = false;
        var infracore = GameData.Instance.InfracoreTable.Values.Where(x => x.Grade == InfraCoreLvl).FirstOrDefault();


        // Check weekly reset
        if (ShouldResetWeekly())
        {
            Logging.WriteLine("Resetting weekly user data...", LogType.Warning);

            LastWeeklyReset = DateTime.UtcNow;
            var currentSeasonData = ResetableData.SimRoomData.CurrentSeasonData;
            currentSeasonData.LatestOption = new();
            ResetableData = new()
            {
                SimRoomData = new()
                {
                    // Retain old LegacyBuffs data and currentSeason data
                    CurrentDifficulty = ResetableData.SimRoomData.CurrentDifficulty,
                    CurrentChapter = ResetableData.SimRoomData.CurrentChapter,
                    CurrentSeasonData = currentSeasonData,
                }
            };
            needsSave = true;
        }

        // Check daily reset
        if (ShouldResetUser())
        {
            Logging.WriteLine("Resetting daily user data...", LogType.Warning);

            LastReset = DateTime.UtcNow;
            ResetableData = new()
            {
                SimRoomData = new()
                {
                    LegacyBuffs = ResetableData.SimRoomData.LegacyBuffs, // Retain old LegacyBuffs data
                    CurrentDifficulty = ResetableData.SimRoomData.CurrentDifficulty,
                    CurrentChapter = ResetableData.SimRoomData.CurrentChapter,
                    CurrentSeasonData = ResetableData.SimRoomData.CurrentSeasonData,
                }
            };

            DispatchResetCount = 0;
            ResetableData.DispatchCount = GetDispatchCount() + infracore.FunctionList[1].Function;
           // ResetableData.DailyCounselCount[1] = 3 + infracore.FunctionList[2].Function;           

            needsSave = true;
        }


        if (needsSave)
        {
            GameContext.Instance.SaveChanges();
        }
    }
    public int GetDateDay()
    {
        DateTime dateTime = DateTime.UtcNow.AddHours(4);
        return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
    }

    public int GetDispatchCount()
    {
        int dis1 = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.Dispatch && x.DispatchBoardLv == DispatchLv).FirstOrDefault().DispatchMax;
        int dis2 = GameData.Instance.DispatchBoardTable.Values.Where(x => x.DispatchType == DispatchType.DispatchCollection && x.DispatchBoardLv == DispatchCollectionLv).FirstOrDefault()?.DispatchMax ?? 0;
        int dis3 = GameData.Instance.DispatchBoardTable.Values
            .Where(x => x.DispatchType == DispatchType.DispatchFavorite && x.DispatchBoardLv == DispatchFavoriteLv).FirstOrDefault()?.DispatchMax ?? 0;

        return dis1 + dis2 + dis3;
    }
    public static int GenerateMsn()
    {
        long timestamp = DateTime.UtcNow.Ticks;
        int seed = (int)(timestamp & 0x7FFFFFFF) ^ (int)(timestamp >> 32);
        var random = new Random(seed);
        return random.Next(100000000, 1000000000);
    }
}
