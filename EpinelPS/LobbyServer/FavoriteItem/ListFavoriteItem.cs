using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/list")]
    public class ListFavoriteItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListFavoriteItem>();
            var user = GetUser();

            var response = new ResListFavoriteItem();

            await WriteDataAsync(response);
        }
    }
}
