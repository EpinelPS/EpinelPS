using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/password-door/list")]
public class ListArchiveFieldPasswordDoor : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListArchiveFieldPasswordDoorData req = await ReadData<ReqListArchiveFieldPasswordDoorData>();
        User user = GetUser();

        ResListArchiveFieldPasswordDoorData response = new();

        if (user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
        {
            response.AcquiredFieldPasswordIdList.AddRange(field.AcquiredPasswordList);
            response.UnlockedFieldPasswordDoorIdList.AddRange(field.UnlockedDoorList);
        }

        await WriteDataAsync(response);
    }
}
