using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/playlist/create")]
public class CreateJukeboxPlaylist : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCreateJukeboxPlaylist req = await ReadData<ReqCreateJukeboxPlaylist>();
        User user = GetUser();

        var playlist = new NetJukeboxPlaylist
        {
            JukeboxPlaylistUid = user.PlayLists.Count == 0 ? 1 : user.PlayLists.Max(p => p.JukeboxPlaylistUid) + 1,
            Title = req.Title
        };
        user.PlayLists.Add(playlist);
        JsonDb.Save();

        await WriteDataAsync(new ResCreateJukeboxPlaylist { Playlist = playlist });
    }
}
