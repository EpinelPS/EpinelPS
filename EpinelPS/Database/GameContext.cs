
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Database;

public class GameContext : DbContext
{
    /// <summary>
    /// Contains user login information
    /// </summary>
    public DbSet<SdkUser> SdkUsers { get; set; }

    /// <summary>
    /// Contains in-game user information
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Contains trigger information
    /// </summary>
    public DbSet<TriggerModelNew> Triggers { get; set; }

    public DbSet<JukeBoxSetting> JukeBoxSetting { get; set; }
    public DbSet<ResetableDataNew> ResetableDataNew { get; set; }
    public DbSet<WeeklyResetableData> WeeklyResetableData { get; set; }

    /// <summary>
    /// GameContext instance. Should only be used in console thread.
    /// </summary>
    public static GameContext Instance { get; set; } = new();
    public GameContext()
    {
        
    }
    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      /*  modelBuilder.Entity<User>()
            .OwnsOne(u => u.ClearedTutorialData, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
            }).OwnsOne(u => u.ContentsOpenUnlocked, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson()
            ).OwnsOne(u => u.LostSectorData, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.EventInfo, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.LoginEventInfo, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.EventShopBuyCountInfo, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.SoloRaidData, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.MiniGameScenarios, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.MiniGameAzxInfo, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.MiniGameStoryChoice, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.TTSGameData, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.StellarBladeDatas, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.TowerDefenseDatas, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
             .OwnsOne(u => u.Currency, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.UserPassInfo, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.EventMissionInfo, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.FieldInfoNew, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.MapJson, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.ResearchProgress, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.InfraCoreRewardReceived, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.MainQuestData, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.SubQuestData, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.UserTeams, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.MailDatas, ownedNavigationBuilder =>
            ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.TowerProgress, ownedNavigationBuilder =>
        ownedNavigationBuilder.ToJson())
            .OwnsOne(u => u.ResetableData, ownedNavigationBuilder =>
        ownedNavigationBuilder.ToJson());*/
    }
}
