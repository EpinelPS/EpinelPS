using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/get")]
    public class GetLostSectorData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetLostSectorData>();
            var user = GetUser();

            var response = new ResGetLostSectorData();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
