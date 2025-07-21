using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/list")]
    public class ListFavoriteItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListFavoriteItem req = await ReadData<ReqListFavoriteItem>();
            Database.User user = GetUser();

            ResListFavoriteItem response = new();

            await WriteDataAsync(response);
        }
    }
}
