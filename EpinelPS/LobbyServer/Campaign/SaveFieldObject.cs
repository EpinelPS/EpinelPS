using EpinelPS.Database;
using EpinelPS.StaticInfo;
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

            Console.WriteLine("Map ID: " + req.MapId);

            var response = new ResSaveCampaignFieldObject();

            Console.WriteLine($"save {req.MapId} with {req.FieldObject.PositionId}");

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
