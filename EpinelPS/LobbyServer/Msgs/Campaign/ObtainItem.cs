using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Campaign
{
    [PacketPath("/campaign/obtain/item")]
    public class ObtainItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainCampaignItem>();
            var user = GetUser();

            var response = new ResObtainCampaignItem();

            var chapter = GameData.Instance.GetNormalChapterNumberFromFieldName(req.MapId);
            var mod = req.MapId.Contains("hard") ? "Hard" : "Normal";
            var key = chapter + "_" + mod;
            var field = user.FieldInfoNew[key];

            // TODO
            response.Reward = new();



            await WriteDataAsync(response);
        }
    }
}
