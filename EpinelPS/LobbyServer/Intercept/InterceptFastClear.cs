using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/fastclear")]
public class FastClearInterceptData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearIntercept req = await ReadData<ReqFastClearIntercept>();
        var user = GetUser();

        ResFastClearIntercept response = new()
        {
            TicketCount = user.ResetableData.InterceptionTickets,
            MaxTicketCount = JsonDb.Instance.MaxInterceptionCount,
            Damage = 0
        };

        await WriteDataAsync(response);
    }
}
