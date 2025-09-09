using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/list")]
    public class ListFavoriteItemQuests : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListFavoriteItemQuest req = await ReadData<ReqListFavoriteItemQuest>();
            User user = GetUser();
            
            ResListFavoriteItemQuest response = new();
            
            if (user.FavoriteItemQuests == null)
            {
                user.FavoriteItemQuests = new List<NetUserFavoriteItemQuestData>();
            }
            
            foreach (NetUserFavoriteItemQuestData quest in user.FavoriteItemQuests)
            {
                response.FavoriteItemQuests.Add(quest);
            }

            await WriteDataAsync(response);
        }
    }
}