namespace EpinelPS.LobbyServer.Event.CollectSystem;

[GameRequest("/event/collect-system/list-field")]
public class ListField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListFieldEventCollectData req = await ReadData<ReqListFieldEventCollectData>();
        User user = GetUser();

        ResListFieldEventCollectData response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
