
using System.ComponentModel.DataAnnotations.Schema;
using EpinelPS.Data;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class GameUser
{
    // User data
    public ulong ID { get; set; }
    public string Nickname { get; set; } = "Player";
    public string Description { get; set; } = "";
    public DateTime LastAction { get; set; }
    public DateTime BanStart { get; set; }
    public DateTime BanEnd { get; set; }
    public int BanId { get; set; } = 0;
    public virtual ICollection<TriggerModelNew> Triggers { get; set; } = new List<TriggerModelNew>();
    public virtual ICollection<CurrencyModel> Currency { get; set; } = new List<CurrencyModel>();
    public virtual ICollection<ClearedTutorial> Tutorials { get; set; } = new List<ClearedTutorial>();
    public virtual ICollection<CharacterModel> Characters { get; set; } = new List<CharacterModel>();
    public virtual ICollection<TeamModel> Teams { get; set; } = new List<TeamModel>();

    // Campaign
    public int LastNormalStageCleared { get; set; }
    public int LastStoryStageCleared { get; set; }
    public int LastHardStageCleared { get; set; }
    public int LastClearedDifficulty { get; set; }
    public List<string> ViewedScenarios {get;set;} = new List<string>();
    public virtual ICollection<FieldInfo> FieldInfo { get; set; } = new List<FieldInfo>();
    public virtual ICollection<QuestProgress> MainQuestData { get; set; } = new List<QuestProgress>();
    public long[] RepresentationTeamDataNew { get; set; } = new long[5];


    // Profile
    public int ProfileIconId { get; set; } = 30100;
    public bool ProfileIconIsPrism { get; set; } = false;
    public int ProfileFrame { get; set; } = 1;
    public int TitleId { get; set; } = 1;
    public List<int> ProfileCardsData { get; set; } = [];


    // Levels
    public int ExperiencePoint { get; set; }
    public int UserLevel { get; set; } = 1;
    public int InfraCoreExp { get; set; } = 0;
    public int InfraCoreLvl { get; set; } = 1;

    // Items
    public List<int> Memorial { get; set; } = [];
    public List<int> JukeboxBgm { get; set; } = [];
    public List<int> ClaimedJukeboxRewardTriggers { get; set; } = [];

    // Outpost
    public int DispatchLv { get; set; } = 1;
    public int DispatchCollectionLv { get; set; } = 0;
    public int DispatchFavoriteLv { get; set; } = 0;
    public int DispatchResetCount { get; set; } = 0;
    public int OutpostBattleLevelExp { get; set; } = 0;
    public int OutpostBattleLevel { get; set; } = 1;
    public List<int> DispatchClearList { get; set; } = [];
    public DateTime BattleTime { get; set; } = DateTime.UtcNow;

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
        return t;
    }
}
