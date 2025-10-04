using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.Models;
public class ClearedTutorialData
{
    public int Id;
    public int VersionGroup;
    public int GroupId;
    public int ClearedStageId;
    public int NextId;
    public bool SaveTutorial;
}
public class User
{
    // User info
    public string? Username;
    public string? Password;
    public string? PlayerName;
    public ulong ID;
    public long RegisterTime;
    public int LastNormalStageCleared;
    public int LastHardStageCleared;
    public string? Nickname;
    public int ProfileIconId = 39900;
    public bool ProfileIconIsPrism = false;
    public int ProfileFrame = 25;
    public bool IsAdmin = false;
    public bool sickpulls = false;
    public bool IsBanned = false;
    public int TitleId = 1;
    public DateTime BanStart;
    public DateTime BanEnd;
    public int BanId = 0;
    public DateTime LastReset = DateTime.MinValue;

    // Game data
    public List<string> CompletedScenarios = [];
    public Dictionary<string, FieldInfo> FieldInfo = []; // here for backwards compatibility

    public Dictionary<string, FieldInfoNew> FieldInfoNew = [];
    public Dictionary<string, string> MapJson = [];
    public Dictionary<CurrencyType, long> Currency = new() {
            { CurrencyType.ContentStamina, 2 }
        };
    public List<SynchroSlot> SynchroSlots = [];
    public bool SynchroDeviceUpgraded = false;
    public int SynchroDeviceLevel = 200;
    public Dictionary<int, RecycleRoomResearchProgress> ResearchProgress = [];

    public ResetableData ResetableData = new();
    public WeeklyResetableData WeeklyResetableData = new();
    public List<ItemData> Items = [];
    public List<CharacterModel> Characters = [];
    public List<EquipmentAwakeningData> EquipmentAwakenings = [];
    public long[] RepresentationTeamDataNew = [];
    public Dictionary<int, ClearedTutorialData> ClearedTutorialData = [];

    // Outpost buildings data
    public List<NetUserOutpostData> OutpostBuildings = [];

    public NetWallpaperData[] WallpaperList = [];
    public NetWallpaperBackground[] WallpaperBackground = [];
    public NetWallpaperJukeboxFavorite[] WallpaperFavoriteList = [];
    public NetWallpaperPlaylist[] WallpaperPlaylistList = [];
    public NetWallpaperJukebox[] WallpaperJukeboxList = [];
    public List<int> LobbyDecoBackgroundList = [];


    public Dictionary<int, NetUserTeamData> UserTeams = [];
    public Dictionary<int, bool> MainQuestData = [];
    public Dictionary<int, bool> SubQuestData = [];
    public int InfraCoreExp = 0;
    public int InfraCoreLvl = 1;
    public Dictionary<int, bool> InfraCoreRewardReceived = [];
    public UserPointData userPointData = new();
    public DateTime LastLogin = DateTime.UtcNow;
    public DateTime BattleTime = DateTime.UtcNow;

    public NetOutpostBattleLevel OutpostBattleLevel = new() { Level = 1 };
    public int GachaTutorialPlayCount = 0;
    public List<int> CompletedTacticAcademyLessons = [];
    public List<int> CompletedSideStoryStages = [];

    public List<int> Memorial = [];
    public List<int> JukeboxBgm = [];
    public List<NetUserFavoriteItemData> FavoriteItems = [];
    
    public List<NetUserFavoriteItemQuestData> FavoriteItemQuests = [];
    public Dictionary<int, int> TowerProgress = [];

    public JukeBoxSetting LobbyMusic = new() { Location = NetJukeboxLocation.Lobby, TableId = 2, Type = NetJukeboxBgmType.JukeboxTableId };
    public JukeBoxSetting CommanderMusic = new() { Location = NetJukeboxLocation.CommanderRoom, TableId = 5, Type = NetJukeboxBgmType.JukeboxTableId };
    public OutpostBuffs OutpostBuffs = new();
    public Dictionary<int, UnlockData> ContentsOpenUnlocked = [];

    public List<NetStageClearInfo> StageClearHistorys = [];

    public List<BadgeModel> Badges = [];

    public List<NetUserAttractiveData> BondInfo = [];
    public List<TriggerModel> Triggers = [];
    public int LastTriggerId = 1;
    public List<int> CompletedAchievements = [];
    public List<NetMessage> MessengerData = [];
    public ulong LastMessageId = 1;
    public long LastBadgeSeq = 1;
    public Dictionary<int, LostSectorData> LostSectorData = [];

    // Event data
    public Dictionary<int, EventData> EventInfo = [];
    public MogMinigameInfo MogInfo = new();
    public List<NetPlaySodaEachGameInfo> ArcadePlaySodaInfoList = [];
    public NetArcadeMvgData ArcadeInTheMirrorData = new();

    public TriggerModel AddTrigger(Trigger type, int value, int conditionId = 0)
    {
        TriggerModel t = new()
        {
            Id = LastTriggerId++,
            Type = type,
            ConditionId = conditionId,
            CreatedAt = DateTime.UtcNow.AddHours(9).Ticks,
            Value = value
        };

        Triggers.Add(t);

        return t;
    }

    public BadgeModel AddBadge(BadgeContents type, string location)
    {
        // generate unique badge SEQ

        var badge = new BadgeModel()
        {
            BadgeContent = type,
            Location = location,
            BadgeGuid = Guid.NewGuid().ToString(),
            Seq = LastBadgeSeq++
        };

        Badges.Add(badge);

        return badge;
    }


    public void SetQuest(int tId, bool recievedReward)
    {
        if (!MainQuestData.TryAdd(tId, recievedReward))
        {
            MainQuestData[tId] = recievedReward;
            return;
        }
    }

    public void SetSubQuest(int tId, bool recievedReward)
    {
        if (!SubQuestData.TryAdd(tId, recievedReward))
        {
            SubQuestData[tId] = recievedReward;
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
        foreach (var item in FieldInfoNew)
        {
            if (item.Value.CompletedStages.Contains(Id))
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

    public NetMessage CreateMessage(MessengerDialogRecord r, int state = 0)
    {
        var msg = new NetMessage()
        {
            ConversationId = r.ConversationId,
            CreatedAt = DateTime.UtcNow.Ticks,
            MessageId = r.Id,
            Seq = (long)LastMessageId++,
            State = state
        };
        MessengerData.Add(msg);
        return msg;
    }

    public NetMessage CreateMessage(string conversationId, string messageId, int state = 0)
    {
        var msg = new NetMessage()
        {
            ConversationId = conversationId,
            CreatedAt = DateTime.UtcNow.Ticks,
            MessageId = messageId,
            Seq = (long)LastMessageId++,
            State = state
        };
        MessengerData.Add(msg);
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

    public void ResetDataIfNeeded()
    {
        if (!ShouldResetUser()) return;

        Logging.WriteLine("Resetting user...", LogType.Warning);

        LastReset = DateTime.UtcNow;
        ResetableData = new();
        JsonDb.Save();
    }
}