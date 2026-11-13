using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Field;

[GameRequest("/archive/field/save")]
public class SaveArchiveField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveArchiveFieldRawData req = await ReadData<ReqSaveArchiveFieldRawData>();
        User user = GetUser();

        ResSaveArchiveFieldRawData response = new();

        if (string.IsNullOrEmpty(req.MapId))
        {
            await WriteDataAsync(response);
            return;
        }

        if (!user.MapJson.ContainsKey(req.MapId))
        {
            user.MapJson.Add(req.MapId, req.Json ?? "");
        }
        else
        {
            user.MapJson[req.MapId] = req.Json ?? "";
        }

        if (!user.FieldInfoNew.ContainsKey(req.MapId))
        {
            user.FieldInfoNew.Add(req.MapId, new FieldInfoNew());
        }

        await WriteDataAsync(response);
    }
}
