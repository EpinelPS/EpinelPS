using EpinelPS.Data;
using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/getjoinedevent")]
    public class GetJoinedEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetJoinedEvent>();
            //types are defined in EventTypes.cs
            ResGetJoinedEvent response = new();

			// add gacha events from active lobby banners
			EventHelper.AddJoinedEvents(ref response);
			
            await WriteDataAsync(response);
        }
    }
}
