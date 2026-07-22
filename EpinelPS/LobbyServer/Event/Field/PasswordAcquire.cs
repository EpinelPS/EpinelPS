using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/event/field/password/acquire")]
public class PasswordAcquire : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireFieldPassword req = await ReadData<ReqAcquireFieldPassword>();
        User user = GetUser();
        ResAcquireFieldPassword response = new();

        if (GameData.Instance.MapData.TryGetValue(req.MapId, out FieldMapRecord? fieldMap))
        {
            PasswordSpawnerData_Raw? item = fieldMap.PasswordSpawner
                .Where(x => x.PositionId == req.PositionId).FirstOrDefault();
            if (item != null)
            {
                FieldPasswordRecord_Raw? password = GameData.Instance.FieldPasswordTable.Values
                    .Where(x => x.Id == item.PasswordTableId).FirstOrDefault();
                if (user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
                {
                    field.AcquiredPasswordList.Add(password.Id);
                }
            }
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}