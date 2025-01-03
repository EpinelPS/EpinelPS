using EpinelPS.Utils;
using static EpinelPS.LobbyServer.Event.EventConstants;
namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/getjoinedevent")]
    public class GetJoinedEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetJoinedEvent>();
			//types are defined in EventTypes.cs
            var response = new ResGetJoinedEvent();

            response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                Id = 20001,
                EventSystemType = PickupGachaEvent,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });

			// ssr rapi
			response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                Id = 70077,
                EventSystemType = PickupGachaEvent,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 10046,
					EventSystemType = LoginEvent,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 40066,
					EventSystemType = StoryEvent,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 60066,
					EventSystemType = ChallengeModeEvent,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 70078,
					EventSystemType = PickupGachaEvent,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 70079,
					EventSystemType = PickupGachaEvent,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			//full burst day
			
			/*
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 140052,
					EventSystemType = RewardUpEvent,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			*/
			
			//dailies reward up
			
			/*
			response.EventWithJoinData.Add(new NetEventWithJoinData()
			{
				EventData = new NetEventData()
				{
					Id = 170017,
					EventSystemType = TriggerMissionEventReward,
					EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
					EventDisableDate = DateTime.Now.AddDays(20).Ticks,
					EventEndDate = DateTime.Now.AddDays(20).Ticks
				},
				JoinAt = 0
			});
			*/
            await WriteDataAsync(response);
        }
    }
}
