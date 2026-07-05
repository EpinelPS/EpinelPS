
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class GameUser
{
    // General
    public ulong ID { get; set; }
    public string Nickname { get; set; } = "Player";
    public bool IsBanned { get; set; } = false;
    public DateTime BanStart { get; set; }

    public int BanId { get; set; } = 0;
    public DateTime BanEnd { get; set; }
    public DateTime LastAction { get; set; }
    public ICollection<TriggerModelNew> Triggers { get; set; } = new List<TriggerModelNew>();
    public Dictionary<int, ClearedTutorialData> ClearedTutorialData { get; set; } = [];

    // Levels
    public int UserLevel { get; set; } = 1;
    public int ExperiencePoint { get; set; } = 0;

    // Campaign
    public int LastNormalStageCleared { get; set; }
    public int LastStoryStageCleared { get; set; }
    public int LastHardStageCleared { get; set; }
    public int LastClearedDifficulty { get; set; }

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
}
