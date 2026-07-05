
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

            if (!context.Users.Where(x => x.ID == user.ID).Any())
            {
                changed = true;
                var gameUser = new User()
                {
                    ID = user.ID,
                    Nickname = user.Nickname ?? "Guest",
                    LastAction = user.LastLogin
                };
                foreach(var item in user.Triggers)
                {
                    gameUser.Triggers.Add(new TriggerModelNew()
                    {
                        ConditionId = item.ConditionId,
                        CreatedAt = item.CreatedAt,
                        Type = item.Type,
                        User = gameUser,
                        Value = item.Value
                    });
                }

            //    user.Triggers.Clear();
                context.Users.Add(gameUser);
            }
        }

        if (changed) context.SaveChanges();
    }
}
#pragma warning restore 612, 618
