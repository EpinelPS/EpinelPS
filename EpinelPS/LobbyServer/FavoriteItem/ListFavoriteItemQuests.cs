using EpinelPS.Utils;

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

            await WriteDataAsync(response);
        }
    }
}
