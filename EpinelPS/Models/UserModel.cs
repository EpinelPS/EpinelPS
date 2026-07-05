using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.Models;

public class ClearedTutorialData
{
    public int GroupId { get; set; }

    public int Id { get; set; }
    public int VersionGroup{ get; set; }
}

[Obsolete]
public class LegacyUser
{
    // User info
    public string? Username { get; set; }
    [Obsolete]
    public string? Password { get; set; }
    [Obsolete]
    public string? PlayerName { get; set; }
    public ulong ID { get; set; }
    [Obsolete]
    public long RegisterTime { get; set; }
    [Obsolete]
    public string? Nickname { get; set; }
    [Obsolete]
    public bool IsAdmin { get; set; } = false;
    [Obsolete]
    public int LastNormalStageCleared { get; set; }
    [Obsolete]
    public int LastStoryStageCleared { get; set; }
    [Obsolete]
    public int LastHardStageCleared { get; set; }
    [Obsolete]
    public int LastClearedDifficulty { get; set; }
    [Obsolete]
    public int ProfileIconId { get; set; } = 39900;
    [Obsolete]
    public bool ProfileIconIsPrism { get; set; } = false;
    [Obsolete]
    public int ProfileFrame { get; set; } = 25;
    [Obsolete]
    public bool sickpulls { get; set; } = false;
    [Obsolete]
    public bool IsBanned { get; set; } = false;
    [Obsolete]
    public int TitleId { get; set; } = 1;
    [Obsolete]
    public DateTime BanStart { get; set; }
    [Obsolete]
    public DateTime BanEnd { get; set; }
    [Obsolete]
    public int BanId { get; set; } = 0;
    [Obsolete]
    public DateTime LastReset { get; set; } = DateTime.MinValue;
    [Obsolete]
    public DateTime LastWeeklyReset { get; set; } = DateTime.MinValue;
    [Obsolete]
    public bool SynchroDeviceUpgraded { get; set; } = false;
    [Obsolete]
    public int SynchroDeviceLevel { get; set; } = 200;
[Obsolete]
    public int InfraCoreExp { get; set; } = 0;
    [Obsolete]
    public int InfraCoreLvl { get; set; } = 1;
    [Obsolete]
    public int GachaTutorialPlayCount { get; set; } = 0;

[Obsolete]
    public int DispatchLv { get; set; } = 1;
    [Obsolete]
    public int DispatchCollectionLv { get; set; } = 0;
    [Obsolete]
    public int DispatchFavoriteLv { get; set; } = 0;
    [Obsolete]
    public int DispatchResetCount { get; set; } = 0;

    // Game data
    public List<string> CompletedScenarios { get; set; } = [];
    public Dictionary<string, FieldInfo> FieldInfo { get; set; } = []; // here for backwards compatibility

    public Dictionary<string, FieldInfoNew> FieldInfoNew { get; set; } = [];
    public Dictionary<string, string> MapJson { get; set; } = [];
    public Dictionary<CurrencyType, long> Currency { get; set; } = new() {
            { CurrencyType.ContentStamina, 2 }
        };
    public List<SynchroSlot> SynchroSlots { get; set; } = [];
    public Dictionary<int, RecycleRoomResearchProgress> ResearchProgress { get; set; } = [];

    public ResetableData ResetableData { get; set; } = new();
    public WeeklyResetableData WeeklyResetableData { get; set; } = new();
    public List<DbItemData> Items { get; set; } = [];
    public List<CharacterModel> Characters { get; set; } = [];
    public List<EquipmentAwakeningData> EquipmentAwakenings { get; set; } = [];
    public long[] RepresentationTeamDataNew { get; set; } = [];
    [Obsolete]
    public Dictionary<int, ClearedTutorialData> ClearedTutorialDataNew { get; set; } = [];

    // Outpost buildings data
    public List<NetUserOutpostData> OutpostBuildings { get; set; } = [];

    public NetWallpaperData[] WallpaperList { get; set; } = [];
    public NetWallpaperBackground[] WallpaperBackground { get; set; } = [];
    public NetWallpaperJukeboxFavorite[] WallpaperFavoriteList { get; set; } = [];
    public NetWallpaperPlaylist[] WallpaperPlaylistList { get; set; } = [];
    public NetWallpaperJukebox[] WallpaperJukeboxList { get; set; } = [];
    public List<int> LobbyDecoBackgroundList { get; set; } = [];
    public List<int> LiveWallpaperList { get; set; } = [];

    //角色时装
    public List<int> CostumeList { get; set; } = [];

    public List<int> JukeboxThemeList { get; set; } = [];

    //个人面板
    public List<int> StickerList { get; set; } = [];
    public List<int> BackgroundList { get; set; } = [];
    public ProfileCardDecorationLayout DecorationLayout { get; set; } = new();
    public List<int> IconList { get; set; } = [];
    public List<int> FrameList { get; set; } = [];
    public List<int> TitleList { get; set; } = [];
    public Dictionary<long, NetUserMailData> MailDatas { get; set; } = [];       

    public Dictionary<int, NetUserTeamData> UserTeams { get; set; } = [];
    public Dictionary<int, bool> MainQuestData { get; set; } = [];
    public Dictionary<int, bool> SubQuestData { get; set; } = [];
    public Dictionary<int, bool> InfraCoreRewardReceived { get; set; } = [];
    public UserPointData userPointData { get; set; } = new();
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public DateTime BattleTime { get; set; } = DateTime.UtcNow;

    public NetOutpostBattleLevel OutpostBattleLevel { get; set; } = new() { Level = 1 };
    public List<int> CompletedTacticAcademyLessons { get; set; } = [];
    public List<int> CompletedSideStoryStages { get; set; } = [];
    public List<int> ViewedSideStoryStages { get; set; } = [];
    public List<int> ClearedOutpostScenarioIds { get; set; } = [];

    public List<int> Memorial { get; set; } = [];
    public List<int> JukeboxBgm { get; set; } = [];
    public List<NetUserFavoriteItemData> FavoriteItems { get; set; } = [];

    public List<NetUserFavoriteItemQuestData> FavoriteItemQuests { get; set; } = [];
    public Dictionary<int, int> TowerProgress { get; set; } = [];    

    public JukeBoxSetting LobbyMusic { get; set; } = new() { Location = NetJukeboxLocation.Lobby, TableId = 2, Type = NetJukeboxBgmType.JukeboxTableId };
    public JukeBoxSetting CommanderMusic { get; set; } = new() { Location = NetJukeboxLocation.CommanderRoom, TableId = 5, Type = NetJukeboxBgmType.JukeboxTableId };
    public OutpostBuffs OutpostBuffs { get; set; } = new();
    public Dictionary<int, UnlockData> ContentsOpenUnlocked { get; set; } = [];

    public List<NetStageClearInfo> StageClearHistorys { get; set; } = [];

    public List<BadgeModel> Badges { get; set; } = [];

    public List<NetUserAttractiveData> BondInfo { get; set; } = [];
    [Obsolete]
    public List<TriggerModel> Triggers { get; set; } = [];
    public List<int> CompletedAchievements { get; set; } = [];
    public List<NetMessage> MessengerData { get; set; } = [];
    public ulong LastMessageId { get; set; } = 1;
    public long LastBadgeSeq { get; set; } = 1;
    public Dictionary<int, LostSectorData> LostSectorData { get; set; } = [];

    // Event data
    public Dictionary<int, EventData> EventInfo { get; set; } = [];
    public Dictionary<int, LoginEventData> LoginEventInfo { get; set; } = [];
    public Dictionary<int, EventMissionData> EventMissionInfo { get; set; } = []; // key: eventId
    public Dictionary<int, EventShopBuyCountData> EventShopBuyCountInfo { get; set; } = []; // key: eventId
    public MogMinigameInfo MogInfo { get; set; } = new();
    public List<NetPlaySodaEachGameInfo> ArcadePlaySodaInfoList { get; set; } = [];
    public NetArcadeMvgData ArcadeInTheMirrorData { get; set; } = new();

    public Dictionary<int, PassData> UserPassInfo = []; // user pass data, key is PassId

    public List<int> LobbyPrivateBannerIds = [];
    
    // solo raid data
    public Dictionary<int, SoloRaidInfo> SoloRaidData = []; // key: raidId


    //OutpostConditionTable
    public List<int> OutpostConditionList { get; set; } = [];

    // Outpost dispatch
    public List<int> DispatchClearList { get; set; } = [];
    public List<NetSelectableDispatchData> SelectableDispatchData { get; set; } = [];
    public DispatchData UserDispatchData { get; set; } = new();

    public GuildData Guild { get; set; } = new();

    // Minigame data
    public Dictionary<int,MiniGameScenarios> MiniGameScenarios { get;set;  } = new();
    public Dictionary<int, MiniGameAzxData> MiniGameAzxInfo { get; set; } = [];
    public Dictionary<int, MiniGameStoryChoice> MiniGameStoryChoice { get; set; } = [];
    public NetArcadeBBQData BBQInfoData { get; set; } = new();
    public List<NetPlaySodaEachGameInfo> PlaySodaInfoData { get; set; } = [];
    public List<NetRebuildEdenData> RebuildedenData { get; set; } = [];

    public Dictionary<int, TtsDatas> TTSGameData { get; set; } = new();
    public Dictionary<int, StellarBladeDatas> StellarBladeDatas { get; set; } = new();
    public Dictionary<int, TowerDefenseData> TowerDefenseDatas { get; set; } = new();

    // Jukebox
    public List<NetJukeboxPlaylist> PlayLists { get; set; } = [];
    public NetJukeboxFavorite FavoriteSongs { get; set; } = new();
}