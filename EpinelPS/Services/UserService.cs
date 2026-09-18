using EpinelPS.Database;
using EpinelPS.Interfaces;

namespace EpinelPS.Services;

public class UserService(IHttpContextAccessor httpContextAccessor, GameContext context) : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public GameUser? GetUser()
    {
        var id = _httpContextAccessor.HttpContext.Items["UserID"];
        if (id != null && id is ulong u)
        {
            return context.Users.Find(id);
        }
        else
        {
            return null;
        }
    }
}
