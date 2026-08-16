using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/set/playlist")]
public class SetJukeboxBgmPlaylist : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetJukeboxBgmPlaylist req = await ReadData<ReqSetJukeboxBgmPlaylist>();
        User user = GetUser();

        var setting = req.Location == NetJukeboxLocation.Lobby ? user.LobbyMusic : user.CommanderMusic;
        if (user.PlayLists.Any(p => p.JukeboxPlaylistUid == req.JukeboxPlaylistUid))
        {
            setting.Type = NetJukeboxBgmType.JukeboxPlaylist;
            setting.TableId = (int)req.JukeboxPlaylistUid;
            setting.IsShuffle = req.IsShuffle;
            JsonDb.Save();
        }

        await WriteDataAsync(new ResSetJukeboxBgmPlaylist());
    }
}
