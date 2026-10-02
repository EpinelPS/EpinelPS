using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/fastclear")]
public class FastClearInterceptData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearIntercept req = await ReadData<ReqFastClearIntercept>();
        var user = GetUser();

        if (user.ResetableData.InterceptionTickets <= 0)
        {
            Logging.WriteLine("Attempted to fast clear interception when 0 tickets remain", LogType.WarningAntiCheat);
        }
        else
        {
            user.ResetableData.InterceptionTickets--;
        }

        // Get daily recorded damage or max stage damage for quick battle
        user.ResetableData.InterceptDailyClearData.TryGetValue(req.InterceptId, out long maxDamage);
        if (maxDamage <= 0)
        {
            maxDamage = InterceptionHelper.GetMaxDamageForNormalOrSpecial(req.Intercept, req.InterceptId);
        }

        InterceptionClearResult sRes = InterceptionHelper.Clear(user, req.Intercept, req.InterceptId, maxDamage);

        ResFastClearIntercept response = new()
        {
            Damage = maxDamage,
            NormalReward = sRes.NormalReward,
            BonusReward = sRes.BonusReward,
            TicketCount = user.ResetableData.InterceptionTickets,
            MaxTicketCount = JsonDb.Instance.MaxInterceptionCount
        };

        user.AddTrigger(Data.Trigger.InterceptClear, 1);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
