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
        int resetHour = JsonDb.Instance.ResetHourUtcTime;
        DateTime inGameDate = DateTime.UtcNow.AddHours(-resetHour).Date;

        // Continuous epoch counter from official NIKKE launch (Nov 4, 2022) to avoid leap-year discontinuities
        DateTime epoch = new(2022, 11, 4);
        int daysSinceLaunch = (inGameDate - epoch).Days;
        if (daysSinceLaunch < 0) daysSinceLaunch = 0;

        int specialIndex = daysSinceLaunch % specialBosses.Count;
        return specialBosses[specialIndex].Id;
    }
}
