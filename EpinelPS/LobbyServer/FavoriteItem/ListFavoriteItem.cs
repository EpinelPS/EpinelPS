namespace EpinelPS.LobbyServer.FavoriteItem;

[GameRequest("/favoriteitem/list")]
public class ListFavoriteItem : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListFavoriteItem req = await ReadData<ReqListFavoriteItem>();
        User user = GetUser();

        ResListFavoriteItem response = new();

        // Add all user's favorite items to the response
        foreach (NetUserFavoriteItemData favoriteItem in user.FavoriteItems)
        {
            response.FavoriteItems.Add(favoriteItem);
        }

        await WriteDataAsync(response);
    }
}
