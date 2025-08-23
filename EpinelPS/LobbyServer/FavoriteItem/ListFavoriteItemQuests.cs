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
            
            // 如果用户还没有任何收藏品任务数据，则初始化
            if (user.FavoriteItemQuests == null)
            {
                user.FavoriteItemQuests = new List<NetUserFavoriteItemQuestData>();
            }
            
            // 返回用户已接取的任务列表
            foreach (NetUserFavoriteItemQuestData quest in user.FavoriteItemQuests)
            {
                response.FavoriteItemQuests.Add(quest);
            }

            await WriteDataAsync(response);
        }
    }
}