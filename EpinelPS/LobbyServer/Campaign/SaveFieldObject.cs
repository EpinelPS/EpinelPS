using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign;

[GameRequest("/campaign/savefieldobject")]
public class SaveFieldObject : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveCampaignFieldObject req = await ReadData<ReqSaveCampaignFieldObject>();
        User user = GetUser();

        ResSaveCampaignFieldObject response = new();

        Logging.WriteLine($"save {req.MapId} with {req.FieldObject.PositionId}", LogType.Debug);

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfoNew
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        field.CompletedObjects.Add(new CompletedFieldObject() { PositionId = req.FieldObject.PositionId, Json = req.FieldObject.Json, Type = req.FieldObject.Type, User = user });
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
