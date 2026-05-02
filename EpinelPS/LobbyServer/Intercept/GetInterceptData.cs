using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/get")]
public class GetInterceptData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetInterceptData req = await ReadData<ReqGetInterceptData>();

        int specialId = GetCurrentInterceptionIds();

        ResGetInterceptData response = new()
        {
            NormalInterceptGroup = 1,
            SpecialInterceptId = specialId,
            TicketCount = User.ResetableData.InterceptionTickets,
            MaxTicketCount = JsonDb.Instance.MaxInterceptionCount
        };

        await WriteDataAsync(response);
    }

    private int GetCurrentInterceptionIds()
    {
        var specialTable = GameData.Instance.InterceptSpecial;
        var specialBosses = specialTable.Values.Where(x => x.Group == 1).OrderBy(x => x.Order).ToList();

        var dayOfYear = DateTime.UtcNow.DayOfYear;
        var specialIndex = dayOfYear % specialBosses.Count;

        var specialId = specialBosses[specialIndex].Id;
        return specialId;

    }
}
