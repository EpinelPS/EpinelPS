using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/list")]
    public class ListEvents : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetEventList req = await ReadData<ReqGetEventList>();

            // types are defined in EventTypes.cs
            ResGetEventList response = new();

            // add events from active lobby banners
            EventHelper.AddEvents(ref response);

            await WriteDataAsync(response);
        }
    }
}
