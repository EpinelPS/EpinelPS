
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Database;

public class GameContext : DbContext
{
    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
    }
}
