using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/start")]
    public class StartFavoriteItemQuest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStartFavoriteItemQuest req = await ReadData<ReqStartFavoriteItemQuest>();
            User user = GetUser();

            var newQuest = new NetUserFavoriteItemQuestData
            {
                QuestId = req.FavoriteItemQuestId,
                Clear = false,
                Received = false
            };

            user.FavoriteItemQuests.Add(newQuest);

            JsonDb.Save();

            ResStartFavoriteItemQuest response = new();
            await WriteDataAsync(response);
        }


    }
}