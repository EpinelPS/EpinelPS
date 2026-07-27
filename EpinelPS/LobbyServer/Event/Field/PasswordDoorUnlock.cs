using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/event/field/password-door/unlock")]
public class PasswordDoorUnlock : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUnlockFieldPasswordDoor req = await ReadData<ReqUnlockFieldPasswordDoor>();
        User user = GetUser();
        ResUnlockFieldPasswordDoor response = new();

        if (GameData.Instance.MapData.TryGetValue(req.MapId, out FieldMapRecord? fieldMap))
        {
            PasswordDoorSpawnerData_Raw? item = fieldMap.PasswordDoor
                .Where(x => x.PositionId == req.PositionId).FirstOrDefault();
            if (item != null)
            {
                FieldPasswordDoorRecord_Raw? door = GameData.Instance.FieldPasswordDoorTable.Values
                    .Where(x => x.Id == item.PasswordDoorTableId).FirstOrDefault();
                if (user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
                {
                    field.UnlockedDoorList.Add(door.Id);
                }
            }

        }


        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}