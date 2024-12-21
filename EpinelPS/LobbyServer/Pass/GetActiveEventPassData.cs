using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/getactive")]
    public class GetActiveEventPassData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetActiveEventPassData>();

            var response = new ResGetActiveEventPassData();

            // TODO: Support events

            await WriteDataAsync(response);
        }
    }
}
