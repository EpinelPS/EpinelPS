using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/Finish")]
public class FinishAnomalousIntercept : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishInterceptAnomalous req = await ReadData<ReqFinishInterceptAnomalous>();
        User user = GetUser();

        if (user.ResetableData.InterceptionTickets <= 0)
        {
            Logging.WriteLine("Attempted to clear anomalous interception when 0 tickets remain", LogType.WarningAntiCheat);
        }
        else
        {
            user.ResetableData.InterceptionTickets--;
        }

        long damage = req.DamageDealt;

        // Record highest damage this week for this boss to unlock and scale Quick Battle
        if (!user.WeeklyResetableData.InterceptAnomalousClearData.TryGetValue(req.InterceptAnomalousId, out long currentMax) || damage > currentMax)
        {
            user.WeeklyResetableData.InterceptAnomalousClearData[req.InterceptAnomalousId] = damage;
        }

        InterceptionClearResult sRes = InterceptionHelper.ClearAnomalous(user, req.InterceptAnomalousId, damage);

        ResFinishInterceptAnomalous response = new()
        {
            NormalReward = sRes.NormalReward,
            BonusReward = sRes.BonusReward
        };

        user.AddTrigger(Data.Trigger.InterceptClear, 1);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
