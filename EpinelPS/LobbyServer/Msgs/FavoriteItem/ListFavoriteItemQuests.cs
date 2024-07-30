using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/list")]
    public class ListFavoriteItemQuests : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListFavoriteItemQuest>();
            var user = GetUser();

            var response = new ResListFavoriteItemQuest();

            await WriteDataAsync(response);
        }
    }
}
