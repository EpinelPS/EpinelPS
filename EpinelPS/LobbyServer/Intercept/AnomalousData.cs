using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/Anomalous/Data")]
public class GetAnomalousData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqInterceptAnomalousData>();
        var user = GetUser();

        int managerId = 101;
        if (GameData.Instance.InterceptAnomalousManager.Count > 0)
        {
            managerId = GameData.Instance.InterceptAnomalousManager.Keys.FirstOrDefault();
        }

        ResInterceptAnomalousData response = new()
        {
            InterceptAnomalousManagerId = managerId,
            TodayRemainingTickets = user.ResetableData.InterceptionTickets
        };

        // Populate weekly clear records to unlock Quick Battle for cleared bosses
        foreach (var (bossId, maxDamage) in user.WeeklyResetableData.InterceptAnomalousClearData)
        {
            if (maxDamage > 0)
            {
                response.ThisWeekClearData.Add(new ResInterceptAnomalousData.Types.ClearData
                {
                    InterceptAnomalousId = bossId,
                    MaxDamageDealt = maxDamage
                });
            }
        }

        await WriteDataAsync(response);
    }
}
