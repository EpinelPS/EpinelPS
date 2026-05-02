using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Jukebox;

[GameRequest("/jukebox/set/tableid")]
public class SetTableId : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetJukeboxBgmTableId req = await ReadData<ReqSetJukeboxBgmTableId>();
        User user = GetUser();

        ResSetJukeboxBgmTableId response = new();

        if (req.Location == NetJukeboxLocation.CommanderRoom)
        {
            user.CommanderMusic.TableId = req.JukeboxTableId;
            user.CommanderMusic.Type = NetJukeboxBgmType.JukeboxTableId;
        }
        else if (req.Location == NetJukeboxLocation.Lobby)
        {
            user.LobbyMusic.TableId = req.JukeboxTableId;
            user.LobbyMusic.Type = NetJukeboxBgmType.JukeboxTableId;
        }
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
