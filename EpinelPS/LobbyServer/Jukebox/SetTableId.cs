using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox
{
    [PacketPath("/jukebox/set/tableid")]
    public class SetTableId : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetJukeboxBgmTableId>();
            var user = GetUser();

            var response = new ResSetJukeboxBgmTableId();

            if (req.Location == NetJukeboxLocation.NetJukeboxLocationCommanderRoom)
            {
                user.CommanderMusic.TableId = req.JukeboxTableId;
                user.CommanderMusic.Type = NetJukeboxBgmType.NetJukeboxBgmTypeJukeboxTableId;
            }
            else if (req.Location == NetJukeboxLocation.NetJukeboxLocationLobby)
            {
                user.LobbyMusic.TableId = req.JukeboxTableId;
                user.LobbyMusic.Type = NetJukeboxBgmType.NetJukeboxBgmTypeJukeboxTableId;
            }
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
