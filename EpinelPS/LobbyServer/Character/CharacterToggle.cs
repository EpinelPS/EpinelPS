namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/toggle/list")]
public class ListCharacterToggle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListCharacterToggle req = await ReadData<ReqListCharacterToggle>();

        ResListCharacterToggle response = new();

        await WriteDataAsync(response);
    }
}

[GameRequest("/character/toggle/characterinfo")]
public class ToggleCharacterInfo : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqToggleCharacterInfo req = await ReadData<ReqToggleCharacterInfo>();

        ResToggleCharacterInfo response = new();

        await WriteDataAsync(response);
    }
}

[GameRequest("/character/toggle/favoriteitemwallpaper")]
public class ToggleCharacterFavoriteItemWallpaper : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqToggleCharacterFavoriteItemWallpaper req = await ReadData<ReqToggleCharacterFavoriteItemWallpaper>();

        ResToggleCharacterFavoriteItemWallpaper response = new();

        await WriteDataAsync(response);
    }
}
