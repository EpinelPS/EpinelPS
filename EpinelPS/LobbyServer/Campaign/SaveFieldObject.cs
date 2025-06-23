using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign
{
    [PacketPath("/campaign/savefieldobject")]
    public class SaveFieldObject : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSaveCampaignFieldObject>();
            var user = GetUser();

            var response = new ResSaveCampaignFieldObject();

            Logging.WriteLine($"save {req.MapId} with {req.FieldObject.PositionId}", LogType.Debug);

            var field = user.FieldInfoNew[req.MapId];

            field.CompletedObjects.Add(new NetFieldObject() { PositionId = req.FieldObject.PositionId, Json = req.FieldObject.Json, Type = req.FieldObject.Type });
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
