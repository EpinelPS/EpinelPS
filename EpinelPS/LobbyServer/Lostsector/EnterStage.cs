using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/enterstage")]
    public class EnterStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterLostSectorStage req = await ReadData<ReqEnterLostSectorStage>();

            ResEnterLostSectorStage response = new();

            await WriteDataAsync(response);
        }
    }
}
