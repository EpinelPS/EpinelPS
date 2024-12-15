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

             // ice dragon saga
            response.EventList.Add(new NetEventData()
            {
                Id = 81700,
                EventSystemType = 10,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });			

            // banner for new guilotine
            response.EventList.Add(new NetEventData()
            {
                Id = 70076,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });            
			// banner for new maiden
			response.EventList.Add(new NetEventData()
            {
                Id = 70073,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });
            // repickup ludmila
            response.EventList.Add(new NetEventData()
            {
                Id = 70074,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });             
			// repickup mica
            response.EventList.Add(new NetEventData()
            {
                Id = 70075,
                EventSystemType = 6,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });            

            await WriteDataAsync(response);
        }
    }
}
