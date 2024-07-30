using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Pass
{
    [PacketPath("/pass/getactive")]
    public class GetActivePassData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetActivePassData>();

            var response = new ResGetActivePassData();

            // TODO: Support events

            await WriteDataAsync(response);
        }
    }
}
