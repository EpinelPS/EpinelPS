using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox
{
    [PacketPath("/jukebox/record/playhistory")]
    public class JukeboxPlayHistory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRecordJukeboxPlayHistory>();

            var response = new ResRecordJukeboxPlayHistory();
            await WriteDataAsync(response);
        }
    }
}
