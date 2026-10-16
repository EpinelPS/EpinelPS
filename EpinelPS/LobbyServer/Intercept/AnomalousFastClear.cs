using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/FastClear")]
public class AnomalousFastClear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearInterceptAnomalous req = await ReadData<ReqFastClearInterceptAnomalous>();
        User user = GetUser();

        if (user.ResetableData.InterceptionTickets <= 0)
        {
            Logging.WriteLine("Attempted to fast clear anomalous interception when 0 tickets remain", LogType.WarningAntiCheat);
        }
        else
        {
            user.ResetableData.InterceptionTickets--;
        }

        // Get max damage dealt this week for this boss
        user.WeeklyResetableData.InterceptAnomalousClearData.TryGetValue(req.InterceptAnomalousId, out long maxDamage);
        if (maxDamage <= 0)
        {
            maxDamage = InterceptionHelper.GetMaxDamageForAnomalous(req.InterceptAnomalousId);
        }

        InterceptionClearResult sRes = InterceptionHelper.ClearAnomalous(user, req.InterceptAnomalousId, maxDamage);

        ResFastClearInterceptAnomalous response = new()
        {
            DamageDealt = maxDamage,
            NormalReward = sRes.NormalReward,
            BonusReward = sRes.BonusReward,
            TodayRemainingTickets = user.ResetableData.InterceptionTickets
        };

        user.AddTrigger(Data.Trigger.InterceptClear, 1);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
