using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/playlist/set")]
public class SetJukeboxPlaylist : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetJukeboxPlaylist req = await ReadData<ReqSetJukeboxPlaylist>();
        User user = GetUser();

        ResSetJukeboxPlaylist response = new();
        var playlist = user.PlayLists.FirstOrDefault(p => p.JukeboxPlaylistUid == req.JukeboxPlaylistUid);
        if (playlist != null)
        {
            playlist.Songs.Clear();
            playlist.Songs.AddRange(req.Songs);
            JsonDb.Save();
            response.Playlist = playlist;

            // if this playlist is set as BGM somewhere, its contents changed
            foreach (var location in new[] { NetJukeboxLocation.Lobby, NetJukeboxLocation.CommanderRoom })
            {
                var setting = location == NetJukeboxLocation.Lobby ? user.LobbyMusic : user.CommanderMusic;
                if (setting.Type == NetJukeboxBgmType.JukeboxPlaylist && setting.TableId == req.JukeboxPlaylistUid)
                {
                    response.JukeboxBgm.Add(JukeboxUtils.BuildCurrentBgm(user, location));
                    break;
                }
            }
        }

        await WriteDataAsync(response);
    }
}
