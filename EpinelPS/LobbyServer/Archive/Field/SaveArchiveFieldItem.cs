using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/item/save")]
public class SaveArchiveFieldItem : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveArchiveFieldObject req = await ReadData<ReqSaveArchiveFieldObject>();
        User user = GetUser();

        ResSaveArchiveFieldObject response = new();

        if (string.IsNullOrEmpty(req.MapId))
        {
            await WriteDataAsync(response);
            return;
        }

        if (!user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? fieldInfo))
        {
            fieldInfo = new FieldInfoNew();
            user.FieldInfoNew.Add(req.MapId, fieldInfo);
        }

        if (req.InteractionData != null)
        {
            var existingIndex = fieldInfo.CompletedObjects.FindIndex(
                x => x.PositionId == req.InteractionData.PositionId && x.Type == req.InteractionData.Type);

            if (existingIndex != -1)
            {
                fieldInfo.CompletedObjects[existingIndex].Json = req.InteractionData.Json ?? "";
            }
            else
            {
                fieldInfo.CompletedObjects.Add(new NetFieldObject
                {
                    PositionId = req.InteractionData.PositionId ?? "",
                    Type = req.InteractionData.Type,
                    Json = req.InteractionData.Json ?? ""
                });
            }
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
