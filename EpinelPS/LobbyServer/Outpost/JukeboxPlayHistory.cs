using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Jukebox
{
    [PacketPath("/jukebox/record/playhistory")]
    public class JukeboxPlayHistory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = ReadData<ReqRecordJukeboxPlayHistory>();

            var response = new ResRecordJukeboxPlayHistory();
            await WriteDataAsync(response);
        }
    }
}
