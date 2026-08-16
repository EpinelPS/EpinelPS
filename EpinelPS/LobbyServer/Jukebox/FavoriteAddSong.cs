using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/favorite/addsong")]
public class AddJukeboxFavoriteSong : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddJukeboxFavoriteSong req = await ReadData<ReqAddJukeboxFavoriteSong>();
        User user = GetUser();

        if (!user.FavoriteSongs.Songs.Any(s => s.JukeboxTableId == req.JukeboxTableId))
        {
            user.FavoriteSongs.Songs.Add(new NetJukeboxPlaylistSong
            {
                JukeboxTableId = req.JukeboxTableId,
                Order = user.FavoriteSongs.Songs.Count == 0 ? 1 : user.FavoriteSongs.Songs.Max(s => s.Order) + 1
            });
            JsonDb.Save();
        }

        await WriteDataAsync(new ResAddJukeboxFavoriteSong { FavoriteSongs = user.FavoriteSongs });
    }
}
