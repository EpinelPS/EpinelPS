using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/stage/enter")]
    public class EnterFavoriteItemQuestStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterFavoriteItemQuestStage req = await ReadData<ReqEnterFavoriteItemQuestStage>();
            User user = GetUser();

            user.AddTrigger(Trigger.CampaignStart, 1, req.StageId);

            JsonDb.Save();

            ResEnterFavoriteItemQuestStage response = new();
            await WriteDataAsync(response);
        }
    }
}
