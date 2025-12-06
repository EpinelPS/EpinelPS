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
            User user = GetUser();

            // add events from active lobby banners
            EventHelper.AddEvents(user, ref response);

            await WriteDataAsync(response);
        }
    }
}
