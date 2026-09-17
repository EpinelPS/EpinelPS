
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
    /// Long-lived, set once at startup via <see cref="SetInstance"/> - do NOT set this from
    /// the constructor, as DI creates short-lived scoped instances per request that get
    /// disposed when the request ends, leaving a dangling disposed instance behind.
    /// </summary>
    public static GameContext Instance { get; private set; } = null!;
    public static DbContextOptions<GameContext>? Options { get; private set; }
    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
        Options = options;
    }

    public static void InitializeOptions(string connectionString, string? connectionType)
    {
        var builder = new DbContextOptionsBuilder<GameContext>();
        switch (connectionType?.ToLowerInvariant())
        {
            case "sql":
                builder.UseSqlServer(connectionString);
                break;
            case "mysql":
                builder.UseMySQL(connectionString);
                break;
            case "npgsql":
                builder.UseNpgsql(connectionString);
                break;
            default:
                builder.UseSqlite(connectionString);
                break;
        }
        Options = builder.Options;
    }

    public static void SetInstance(GameContext context)
    {
        Instance = context;
    }

    public static GameContext CreateNew()
    {
        if (Options == null)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(baseDir, "epinelps.db");
            InitializeOptions($"Data Source=\"{dbPath}\"", "sqlite");
        }
        return new GameContext(Options);
    }
}
