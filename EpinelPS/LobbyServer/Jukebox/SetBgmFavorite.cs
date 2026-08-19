using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/set/favorite")]
public class SetJukeboxBgmFavorite : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetJukeboxBgmFavorite req = await ReadData<ReqSetJukeboxBgmFavorite>();
        User user = GetUser();

        var setting = req.Location == NetJukeboxLocation.Lobby ? user.LobbyMusic : user.CommanderMusic;
        setting.Type = NetJukeboxBgmType.JukeboxFavorite;
        setting.IsShuffle = req.IsShuffle;
        JsonDb.Save();

        await WriteDataAsync(new ResSetJukeboxBgmFavorite());
    }
}
