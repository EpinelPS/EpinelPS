
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Database;

public static class DbInitializer
{
    public static void Initialize(GameContext context)
    {
        context.Database.EnsureCreated();
    }
}
