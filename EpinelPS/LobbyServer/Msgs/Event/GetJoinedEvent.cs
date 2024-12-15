using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/getjoinedevent")]
    public class GetJoinedEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetJoinedEvent>();

            var response = new ResGetJoinedEvent();
            response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                    Id = 81301,
                    EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                    EventEndDate = DateTime.Now.AddDays(20).Ticks,
                    EventSystemType = 34
                }
            });
            // TODO
            response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                    Id = 81400,
                    EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                    EventEndDate = DateTime.Now.AddDays(20).Ticks,
                    EventSystemType = 10
                }
            });

            response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                    Id = 81401,
                    EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                    EventEndDate = DateTime.Now.AddDays(20).Ticks,
                    EventSystemType = 35
                },
                JoinAt = 0
            });

            response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                    Id = 20001,
                    EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                    EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                    EventEndDate = DateTime.Now.AddDays(20).Ticks,
                    EventSystemType = 1
                },
                JoinAt = 0
            });
			// new guilotine banner
			response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
					Id = 70076,
					EventSystemType = 6,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });	
			// new maiden banner
			response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                Id = 70073,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });			
			// repickup ludmila
			response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                Id = 70074,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });		
			// repickup mica
			response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                Id = 70075,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });
            
            await WriteDataAsync(response);
        }
    }
}
