using EpinelPS.Database;

namespace EpinelPS.LobbyServer.FavoriteItem;

[GameRequest("/favoriteitem/quest/start")]
public class StartFavoriteItemQuest : LobbyMessage
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