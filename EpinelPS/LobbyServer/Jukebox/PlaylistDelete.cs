using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/playlist/delete")]
public class DeleteJukeboxPlaylist : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqDeleteJukeboxPlaylist req = await ReadData<ReqDeleteJukeboxPlaylist>();
        User user = GetUser();

        ResDeleteJukeboxPlaylist response = new();
        var playlist = user.PlayLists.FirstOrDefault(p => p.JukeboxPlaylistUid == req.JukeboxPlaylistUid);
        if (playlist != null)
        {
            user.PlayLists.Remove(playlist);

            // reset any location whose BGM was this playlist back to its default song
            foreach (var location in new[] { NetJukeboxLocation.Lobby, NetJukeboxLocation.CommanderRoom })
            {
                var setting = location == NetJukeboxLocation.Lobby ? user.LobbyMusic : user.CommanderMusic;
                if (setting.Type == NetJukeboxBgmType.JukeboxPlaylist && setting.TableId == req.JukeboxPlaylistUid)
                {
                    setting.Type = NetJukeboxBgmType.JukeboxTableId;
                    setting.TableId = location == NetJukeboxLocation.Lobby ? 2 : 5;
                    setting.IsShuffle = false;
                    response.JukeboxBgm.Add(JukeboxUtils.BuildCurrentBgm(user, location));
                }
            }

            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
