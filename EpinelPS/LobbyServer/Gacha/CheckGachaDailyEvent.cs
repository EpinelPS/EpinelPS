using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/event/check")]
public class CheckGachaDailyEvent : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckDailyFreeGacha req = await ReadData<ReqCheckDailyFreeGacha>();
        ResCheckDailyFreeGacha response = new();

        if (GameData.Instance.gachaTypes.TryGetValue(req.GachaId, out GachaTypeRecord? gacha)
            && gacha.DailyFreeGachaEventId != 0
            && GameData.Instance.eventManagers.TryGetValue(gacha.DailyFreeGachaEventId, out EventManagerRecord? dailyFreeEvent))
        {
            response.FreeCount = 1;
            response.EventData = new NetEventData()
            {
                Id = dailyFreeEvent.Id,
                EventSystemType = (int)dailyFreeEvent.EventSystemType,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.UtcNow.AddDays(30).Ticks,
                EventDisableDate = DateTime.UtcNow.AddDays(30).Ticks
            };
        }

        await WriteDataAsync(response);
    }
}
