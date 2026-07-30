using EpinelPS.Data;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/event/check")]
public class CheckGachaDailyEvent : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckDailyFreeGacha req = await ReadData<ReqCheckDailyFreeGacha>();
               

        ResCheckDailyFreeGacha response = new ResCheckDailyFreeGacha();

        var user = GetUser();

        try
        {
            var evntFree = GameData.Instance.eventManagers[GameData.Instance.gachaTypes[req.GachaId].DailyFreeGachaEventId];

            response.FreeCount = user.GachaDailyFreePulls.Contains(evntFree.Id) ? 0 : 1 ;
            response.EventData = new NetEventData()
            {
                Id = evntFree.Id,
                EventSystemType = (int)evntFree.EventSystemType,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            };
        }
        catch (Exception ex)
        {
            Logging.WriteLine("[CheckGachaDailyEvent] Event was not found in the database.");
        }

        await WriteDataAsync(response);
    }
}
