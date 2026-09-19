
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
    
    private static string ConnectionString = "";
    private static string ConnectionType = "";
    public GameContext()
    {

    }
    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
    }

    public static GameContext CreateNew()
    {
        return new GameContext();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options = options.UseLazyLoadingProxies();
        switch (ConnectionType?.ToLowerInvariant())
        {
            case "sql":
                options.UseSqlServer(ConnectionString);
                break;

            case "mysql":
                options.UseMySQL(ConnectionString);
                break;

            case "npgsql":
                options.UseNpgsql(ConnectionString);
                break;

            default:
                options.UseSqlite(ConnectionString);
                break;
        }
    }

    internal static void SetOptions(string connectionString, string connectionType)
    {
        ConnectionString = connectionString;
        ConnectionType = connectionType;
    }
}
