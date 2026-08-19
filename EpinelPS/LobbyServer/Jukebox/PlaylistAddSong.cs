using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/playlist/addsong")]
public class AddSongJukeboxPlaylist : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddSongJukeboxPlaylist req = await ReadData<ReqAddSongJukeboxPlaylist>();
        User user = GetUser();

        ResAddSongJukeboxPlaylist response = new();
        var playlist = user.PlayLists.FirstOrDefault(p => p.JukeboxPlaylistUid == req.JukeboxPlaylistUid);
        if (playlist != null && !playlist.Songs.Any(s => s.JukeboxTableId == req.JukeboxTableId))
        {
            playlist.Songs.Add(new NetJukeboxPlaylistSong
            {
                JukeboxTableId = req.JukeboxTableId,
                Order = playlist.Songs.Count == 0 ? 1 : playlist.Songs.Max(s => s.Order) + 1
            });
            JsonDb.Save();
            response.Playlist = playlist;
        }

        await WriteDataAsync(response);
    }
}
