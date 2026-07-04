
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Database;

public class GameContext : DbContext
{
    /// <summary>
    /// Contains user information
    /// </summary>
    public DbSet<SdkUser> SdkUsers { get; set; }

    /// <summary>
    /// GameContext instance. Should only be used in console thread.
    /// </summary>
    public static GameContext Instance { get; set; }
    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
        Instance = this;
    }
}
