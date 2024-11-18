using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Gacha
{
    [PacketPath("/gacha/event/check")]
    public class CheckGachaDailyEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckDailyFreeGacha>();
            var response = new ResCheckDailyFreeGacha();

            // TODO implement
            response.FreeCount = 1;
            response.EventData = new NetEventData()
            {
                Id = 80004,
                EventSystemType = 21,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            };
			// this is net event data i think it should be the same as in list events and get joined event only for free pull gacha event 

            await WriteDataAsync(response);
        }
    }
}
