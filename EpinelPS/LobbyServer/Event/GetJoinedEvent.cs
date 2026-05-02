namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/getjoinedevent")]
public class GetJoinedEvent : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetJoinedEvent>();
        //types are defined in EventTypes.cs
        ResGetJoinedEvent response = new();
        User user = GetUser();

        // add gacha events from active lobby banners
        EventHelper.AddJoinedEvents(user, ref response);

        await WriteDataAsync(response);
    }
}
