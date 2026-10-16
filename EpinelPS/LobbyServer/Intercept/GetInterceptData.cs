using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/get")]
public class GetInterceptData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetInterceptData>();
        var user = GetUser();
        user.ResetDataIfNeeded();

        int specialId = GetCurrentSpecialInterceptionId();
        ResGetInterceptData response = new()
        {
            NormalInterceptGroup = 1,
            SpecialInterceptId = specialId,
            TicketCount = user.ResetableData.InterceptionTickets,
            MaxTicketCount = JsonDb.Instance.MaxInterceptionCount
        };

        await WriteDataAsync(response);
    }

    public static int GetCurrentSpecialInterceptionId()
    {
        var specialTable = GameData.Instance.InterceptSpecial;
        var specialBosses = specialTable.Values.Where(x => x.Group == 1).OrderBy(x => x.Order).ToList();
        if (specialBosses.Count == 0) return 1;

        // Align with official daily reset time (JsonDb.Instance.ResetHourUtcTime = 20:00 UTC / 05:00 KST)
        // Daily content now runs on a one-hour cycle, so the boss rotates every hour.
        DateTime inGameDate = DateTime.UtcNow.Date;
        int hourlyOffset = DateTime.UtcNow.Hour;

        // Continuous epoch counter from official NIKKE launch (Nov 4, 2022) to avoid leap-year discontinuities
        DateTime epoch = new(2022, 11, 4);
        int daysSinceLaunch = (inGameDate - epoch).Days;
        if (daysSinceLaunch < 0) daysSinceLaunch = 0;

        int cycleSinceLaunch = daysSinceLaunch * 24 + hourlyOffset;
        int specialIndex = cycleSinceLaunch % specialBosses.Count;
        return specialBosses[specialIndex].Id;
    }
}
