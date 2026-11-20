using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/enter")]
public class EnterArchiveField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArchiveField req = await ReadData<ReqEnterArchiveField>();
        User user = GetUser();

        ResEnterArchiveField response = new()
        {
            Field = new()
        };

        if (!user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
        {
            field = new FieldInfoNew();
            user.FieldInfoNew.Add(req.MapId, field);
        }

        foreach (int stage in field.CompletedStages)
        {
            response.Field.Stages.Add(new NetFieldStageData() { StageId = stage });
        }

        foreach (NetFieldObject obj in field.CompletedObjects)
        {
            if (obj == null) continue;
            if (obj.Type == 1)
            {
                response.Field.Objects.Add(obj);
            }
            else
            {
                response.NonResettableFieldObjects.Add(new NetNonResettableFieldObject
                {
                    PositionId = obj.PositionId,
                    Type = obj.Type,
                    Json = obj.Json
                });
            }
        }

        if (user.MapJson.TryGetValue(req.MapId, out string? mapJson))
        {
            response.Json = mapJson;
        }

        await WriteDataAsync(response);
    }
}
