namespace EpinelPS.LobbyServer.Event;

[GameRequest("/eventfield/enter")]
public class EnterEventField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterEventField req = await ReadData<ReqEnterEventField>();
        User user = GetUser();

        ResEnterEventField response = new()
        {
            Field = new()
        };



        // Retrieve collected objects and completed stages
        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfoNew
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        response.Json = field.PositionJson;

        foreach (int stage in field.CompletedStages)
        {
            response.Field.Stages.Add(new NetFieldStageData() { StageId = stage });
        }
        foreach (var obj in field.CompletedObjects)
        {
            if (obj == null) continue;
            if (obj.Type == 1)
                response.Field.Objects.Add(new NetFieldObject()
                {
                    PositionId = obj.PositionId,
                    ActionAt = obj.ActionAt.Ticks,
                    Json = obj.Json,
                    Type = obj.Type
                });
        }

        await WriteDataAsync(response);
    }
}
