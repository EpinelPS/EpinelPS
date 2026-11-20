using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/password-door/unlock")]
public class UnlockArchiveFieldPasswordDoor : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUnlockArchiveFieldPasswordDoor req = await ReadData<ReqUnlockArchiveFieldPasswordDoor>();
        User user = GetUser();
        ResUnlockArchiveFieldPasswordDoor response = new();

        if (GameData.Instance.MapData.TryGetValue(req.MapId, out FieldMapRecord? fieldMap))
        {
            PasswordDoorSpawnerData_Raw? item = fieldMap.PasswordDoor
                .FirstOrDefault(x => x.PositionId == req.PositionId);
            if (item != null)
            {
                FieldPasswordDoorRecord_Raw? door = GameData.Instance.FieldPasswordDoorTable.Values
                    .FirstOrDefault(x => x.Id == item.PasswordDoorTableId);
                if (door != null)
                {
                    if (!user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
                    {
                        field = new FieldInfoNew();
                        user.FieldInfoNew.Add(req.MapId, field);
                    }

                    if (!field.UnlockedDoorList.Contains(door.Id))
                    {
                        field.UnlockedDoorList.Add(door.Id);
                    }
                }
            }
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
