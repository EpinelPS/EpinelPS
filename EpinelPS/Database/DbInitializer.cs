
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Database;

#pragma warning disable 612, 618
public static class DbInitializer
{
    public static void Initialize(GameContext context)
    {
        Logging.WriteLine("Initializing & migrating database...");
        context.Database.Migrate();
        context.Database.EnsureCreated();

        context.SaveChanges();
    }
}
#pragma warning restore 612, 618
