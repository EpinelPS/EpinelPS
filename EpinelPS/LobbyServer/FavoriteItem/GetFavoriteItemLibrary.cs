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
            User user = GetUser();

            foreach (NetUserFavoriteItemData favoriteItem in user.FavoriteItems)
            {
                NetFavoriteItemLibraryElement libraryElement = new NetFavoriteItemLibraryElement
                {
                    Tid = favoriteItem.Tid,
                    ReceivedAt = DateTime.UtcNow.Ticks // Use current time as received time
                };
                response.FavoriteItemLibrary.Add(libraryElement);
            }

            await WriteDataAsync(response);
        }
    }
}
