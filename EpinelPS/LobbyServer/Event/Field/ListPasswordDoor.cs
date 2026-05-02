namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/event/field/password-door/list")]
public class ListPasswordDoor : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListFieldPasswordDoorData req = await ReadData<ReqListFieldPasswordDoorData>();
        User user = GetUser();

        ResListFieldPasswordDoorData response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
