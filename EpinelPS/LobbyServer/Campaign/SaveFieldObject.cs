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

            var chapter = GameData.Instance.GetNormalChapterNumberFromFieldName(req.MapId);
            var mod = req.MapId.Contains("hard") ? "Hard" : "Normal";
            var key = chapter + "_" + mod;
            var field = user.FieldInfoNew[key];

            field.CompletedObjects.Add(new NetFieldObject() { PositionId = req.FieldObject.PositionId, Json = req.FieldObject.Json, Type = req.FieldObject.Type });
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
