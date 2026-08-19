namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/jukebox/playlist/get")]
public class JukeboxPlaylistGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetJukeboxPlaylist req = await ReadData<ReqGetJukeboxPlaylist>();
        User user = GetUser();

        ResGetJukeboxPlaylist response = new()
        {
            FavoriteSongs = user.FavoriteSongs
        };
        response.Playlists.AddRange(user.PlayLists);

        await WriteDataAsync(response);
    }
}
