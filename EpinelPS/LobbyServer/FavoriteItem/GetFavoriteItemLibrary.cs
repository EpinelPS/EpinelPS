using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/library")]
    public class GetFavoriteItemLibrary : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetFavoriteItemLibrary req = await ReadData<ReqGetFavoriteItemLibrary>();

            ResGetFavoriteItemLibrary response = new();
            Database.User user = GetUser();


            await WriteDataAsync(response);
        }
    }
}
