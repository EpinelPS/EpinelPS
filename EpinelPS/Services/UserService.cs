using EpinelPS.Database;
using EpinelPS.Interfaces;

namespace EpinelPS.Services;

public class UserService(IHttpContextAccessor httpContextAccessor, GameContext GameContext) : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly GameContext _db = GameContext;

    public User? GetUser()
    {
        var id = _httpContextAccessor.HttpContext.Items["UserID"];
        if (id != null && id is ulong u)
        {
            return _db.Users.Find(u);
        }
        else
        {
            return null;
        }
    }
}
