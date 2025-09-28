using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox
{
    [PacketPath("/jukebox/set/tableId")]
    public class SetTableId : LobbyMsgHandler
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
}
