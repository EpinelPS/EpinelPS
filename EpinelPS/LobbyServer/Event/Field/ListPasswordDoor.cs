using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/event/field/password-door/list")]
public class ListPasswordDoor : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListFieldPasswordDoorData req = await ReadData<ReqListFieldPasswordDoorData>();
        User user = GetUser();

        ResListFieldPasswordDoorData response = new();

        if (user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
        {
            response.AcquiredFieldPasswordIdList.AddRange(field.AcquiredPasswordList);
            response.UnlockedFieldPasswordDoorIdList.AddRange(field.UnlockedDoorList);
        }

        // TODO

        await WriteDataAsync(response);
    }
}
