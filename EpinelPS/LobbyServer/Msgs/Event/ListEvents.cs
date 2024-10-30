using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/list")]
    public class ListEvents : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventList>();

            // 10: FieldHubEvent

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
response.EventList.Add(new NetEventData()
            {
                Id = 81600,
                EventSystemType = 10,
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
