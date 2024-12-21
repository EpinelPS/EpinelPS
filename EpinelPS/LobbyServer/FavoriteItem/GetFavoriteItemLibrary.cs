using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/library")]
    public class GetFavoriteItemLibrary : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFavoriteItemLibrary>();

            var response = new ResGetFavoriteItemLibrary();
            var user = GetUser();


            await WriteDataAsync(response);
        }
    }
}
