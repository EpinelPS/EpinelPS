
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
    public DbSet<GameUser> Users { get; set; }

    /// <summary>
    /// Contains trigger information
    /// </summary>
    public DbSet<TriggerModelNew> Triggers { get; set; }

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
    modelBuilder.Entity<GameUser>()
        .OwnsOne(u => u.ClearedTutorialData, ownedNavigationBuilder =>
        {
            ownedNavigationBuilder.ToJson();
        });
    }
}
