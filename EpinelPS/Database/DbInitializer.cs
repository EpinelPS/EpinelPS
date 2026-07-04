
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Database;

#pragma warning disable 612, 618
public static class DbInitializer
{
    public static void Initialize(GameContext context)
    {
        context.Database.EnsureCreated();

        bool changed = false;

        // migrate sdk users to sqlite DB
        foreach(var user in JsonDb.Instance.Users)
        {
            if (!context.SdkUsers.Where(x => x.ID == user.ID).Any())
            {
                changed = true;
                context.SdkUsers.Add(new SdkUser()
                {
                    ID = user.ID,
                    PasswordHash = user.Password ?? "",
                    Email = user.Username ?? "",
                    RegisterTime = user.RegisterTime,
                    PlayerName = user.PlayerName ?? "Player_NULL",
                    IsAdmin = user.IsAdmin,
                    LastLogin = user.LastLogin
                });
            }
        }

        if (changed) context.SaveChanges();
    }
}
#pragma warning restore 612, 618
