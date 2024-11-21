using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/list")]
    public class ListEvents : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventList>();
            

            // Types
            // 2: LoginEvent
            // 5: StoryEvent
            // 10: FieldHubEvent
            // 11: ShopEvent
            // 20: ChallengeModeEvent
            // 38: MVGMiniGame


            var response = new ResGetEventList();
/*
            response.EventList.Add(new NetEventData()
            {
                Id = 81301,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventSystemType = 34
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 81500,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventSystemType = 10
            });
            

            // TODO: Support events

            response.EventList.Add(new NetEventData()
            {
                Id = 20001,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventSystemType = 1
            });





            // Island Adventure
            response.EventList.Add(new NetEventData()
            {
                Id = 81400,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 10
            });
            response.EventList.Add(new NetEventData()
            {
                Id = 81401,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 35
            });
            response.EventList.Add(new NetEventData()
            {
                Id = 81402,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 11
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 81403,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 2
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 160011,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 22
            });

            // Aegis the Diver event
            response.EventList.Add(new NetEventData()
            {
                Id = 800001,
                EventSystemType = 36,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 70034,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

*/
            // Old tales
/*
            response.EventList.Add(new NetEventData()
            {
                Id = 81600,
                EventSystemType = 10,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

            // Enable story event 1
            response.EventList.Add(new NetEventData()
            {
                Id = 40061,
                EventSystemType = 5,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

            // Enable story event 2
            response.EventList.Add(new NetEventData()
            {
                Id = 40062,
                EventSystemType = 5,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

            // Enable challenge mode
            response.EventList.Add(new NetEventData()
            {
                Id = 60061,
                EventSystemType = 20,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

            // enable "mini" game
            response.EventList.Add(new NetEventData()
            {
                Id = 81601,
                EventSystemType = 38,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

            // enable shop
            response.EventList.Add(new NetEventData()
            {
                Id = 81602,
                EventSystemType = 11,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });

             // enable login event
            response.EventList.Add(new NetEventData()
            {
                Id = 81603,
                EventSystemType = 2,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });
			
			*/
             // secret garden - system error when trying to enter event idk why
            response.EventList.Add(new NetEventData()
            {
                Id = 40063,
                EventSystemType = 5,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });			
            // Enable challenge mode
            response.EventList.Add(new NetEventData()
            {
                Id = 60063,
                EventSystemType = 20,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });			
			
            // banner for grave i think
            response.EventList.Add(new NetEventData()
            {
                Id = 70071,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });            
			// free pull for grave
			response.EventList.Add(new NetEventData()
            {
                Id = 80005,
                EventSystemType = 21,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });
            // banner for flora
            response.EventList.Add(new NetEventData()
            {
                Id = 70072,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });            

            
            //response.EventList.Add(new NetEventData()
            //{
            //    Id = 40054,
            //    EventSystemType = 5,
            //    EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
            //    EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
            //    EventEndDate = DateTime.Now.AddDays(20).Ticks,
            //    EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            //});
            await WriteDataAsync(response);
        }
    }
}
