using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/password/acquire")]
public class AcquireArchiveFieldPassword : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireArchiveFieldPassword req = await ReadData<ReqAcquireArchiveFieldPassword>();
        User user = GetUser();
        ResAcquireArchiveFieldPassword response = new();

        if (GameData.Instance.MapData.TryGetValue(req.MapId, out FieldMapRecord? fieldMap))
        {
            PasswordSpawnerData_Raw? item = fieldMap.PasswordSpawner
                .FirstOrDefault(x => x.PositionId == req.PositionId);
            if (item != null)
            {
                FieldPasswordRecord_Raw? password = GameData.Instance.FieldPasswordTable.Values
                    .FirstOrDefault(x => x.Id == item.PasswordTableId);
                if (password != null)
                {
                    if (!user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
                    {
                        field = new FieldInfoNew();
                        user.FieldInfoNew.Add(req.MapId, field);
                    }

                    if (!field.AcquiredPasswordList.Contains(password.Id))
                    {
                        field.AcquiredPasswordList.Add(password.Id);
                    }
                }
            }
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
