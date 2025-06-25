using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/enterstage")]
    public class EnterStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterLostSectorStage>();

            var response = new ResEnterLostSectorStage();

            await WriteDataAsync(response);
        }
    }
}
