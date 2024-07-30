using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Stage
{
    [PacketPath("/stage/enterstage")]
    public class EnterStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterStage>();

            var response = new ResEnterStage();

            await WriteDataAsync(response);
        }
    }
}
