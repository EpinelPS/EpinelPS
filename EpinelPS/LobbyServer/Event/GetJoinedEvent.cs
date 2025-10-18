using EpinelPS.Data;
using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/getjoinedevent")]
    public class GetJoinedEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetJoinedEvent req = await ReadData<ReqGetJoinedEvent>();
            //types are defined in EventTypes.cs
            ResGetJoinedEvent response = new();

            response.EventWithJoinData.Add(new NetEventWithJoinData()
            {
                EventData = new NetEventData()
                {
                Id = 20001,
                EventSystemType = (int)EventSystemType.PickupGachaEvent,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks
                },
                JoinAt = 0
            });

			// add gacha events from active lobby banners
			EventHelper.AddJoinedGachaEvents(ref response);
			
            await WriteDataAsync(response);
        }
    }
}
