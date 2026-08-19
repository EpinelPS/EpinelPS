using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/favorite/set")]
public class SetJukeboxFavoriteSongs : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetJukeboxFavoriteSongs req = await ReadData<ReqSetJukeboxFavoriteSongs>();
        User user = GetUser();

        user.FavoriteSongs.Songs.Clear();
        user.FavoriteSongs.Songs.AddRange(req.Songs);
        JsonDb.Save();

        ResSetJukeboxFavoriteSongs response = new() { FavoriteSongs = user.FavoriteSongs };

        // if favorites are set as BGM somewhere, its contents changed
        foreach (var location in new[] { NetJukeboxLocation.Lobby, NetJukeboxLocation.CommanderRoom })
        {
            var setting = location == NetJukeboxLocation.Lobby ? user.LobbyMusic : user.CommanderMusic;
            if (setting.Type == NetJukeboxBgmType.JukeboxFavorite)
            {
                response.JukeboxBgm.Add(JukeboxUtils.BuildCurrentBgm(user, location));
                break;
            }
        }

        await WriteDataAsync(response);
    }
}
