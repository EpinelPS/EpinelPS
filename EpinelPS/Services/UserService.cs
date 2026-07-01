using EpinelPS.Database;
using EpinelPS.Interfaces;

namespace EpinelPS.Services;

public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public User? GetUser()
    {
        var id = _httpContextAccessor.HttpContext.Items["UserID"];
        if (id != null && id is ulong u)
        {
            return JsonDb.GetUser(u);
        }
        else
        {
            return null;
        }
    }
}
