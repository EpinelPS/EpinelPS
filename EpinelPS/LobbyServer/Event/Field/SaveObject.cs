using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/event/field/saveobject")]
public class SaveObject : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveEventFieldNonResettableFieldObject req = await ReadData<ReqSaveEventFieldNonResettableFieldObject>();
        User user = GetUser();
        ResSaveEventFieldNonResettableFieldObject response = new();

        EventFieldRecord? field = GameData.Instance.EventFieldTable.Values
            .Where(x => x.EventId == req.EventId).FirstOrDefault();
        if (field!= null)
        {
            FieldInfoNew? fieldInfo = user.FieldInfoNew[field.FieldDesignMap];
            if (fieldInfo!=null)
            {
                foreach (var item in req.FieldObjects)
                {
                    fieldInfo.CompletedObjects.AddUnique(new NetFieldObject() { PositionId = item.PositionId, Json = item.Json, Type = item.Type });
                }
            }
            else
            {
                user.FieldInfoNew.TryAdd(field.FieldDesignMap, new FieldInfoNew());
                foreach (var item in req.FieldObjects)
                {
                    user.FieldInfoNew[field.FieldDesignMap].CompletedObjects
                        .AddUnique(new NetFieldObject() { PositionId = item.PositionId, Json = item.Json, Type = item.Type });
                }
            }
            
        }       

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}