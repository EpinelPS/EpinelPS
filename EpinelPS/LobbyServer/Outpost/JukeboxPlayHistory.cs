using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/jukebox/record/playhistory")]
    public class JukeboxPlayHistory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqRecordJukeboxPlayHistory req = await ReadData<ReqRecordJukeboxPlayHistory>();

            ResRecordJukeboxPlayHistory response = new();
            await WriteDataAsync(response);
        }
    }
}
