using EpinelPS.Utils;
using static EpinelPS.LobbyServer.Event.EventConstants;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/list")]
    public class ListEvents : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventList>();
            //types are defined in EventTypes.cs
            var response = new ResGetEventList();


			// footsteps walk run
            response.EventList.Add(new NetEventData()
            {
                Id = 40066,
                EventSystemType = StoryEvent,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });             
			response.EventList.Add(new NetEventData()
            {
                Id = 60066,
                EventSystemType = ChallengeModeEvent,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });            
			// ssr rapi
            response.EventList.Add(new NetEventData()
            {
                Id = 70077,
                EventSystemType = PickupGachaEvent,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
            });            
			response.EventList.Add(new NetEventData()
			{
				Id = 10046,
				EventSystemType = LoginEvent,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});

			response.EventList.Add(new NetEventData()
			{
				Id = 20001,
				EventSystemType = DailyMissionEvent,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});
			response.EventList.Add(new NetEventData()
			{
				Id = 20002,
				EventSystemType = DailyMissionEvent,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});

			response.EventList.Add(new NetEventData()
			{
				Id = 70078,
				EventSystemType = PickupGachaEvent,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});
			response.EventList.Add(new NetEventData()
			{
				Id = 70079,
				EventSystemType = PickupGachaEvent,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});
			//full burst day
			
			/*
			response.EventList.Add(new NetEventData()
			{
				Id = 140052,
				EventSystemType = RewardUpEvent,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});
			*/
			
			//dailies reward up
			
			/*
			response.EventList.Add(new NetEventData()
			{
				Id = 170017,
				EventSystemType = TriggerMissionEventReward,
				EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
				EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
				EventEndDate = DateTime.Now.AddDays(20).Ticks,
				EventDisableDate = DateTime.Now.AddDays(20).Ticks
			});
			*/

			
            await WriteDataAsync(response);
        }
    }
}
