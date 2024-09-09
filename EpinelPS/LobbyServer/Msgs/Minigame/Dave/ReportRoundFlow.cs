using EpinelPS.Utils;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/reportdiverroundflow")]
    public class ReportDiverRoundFlow : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqReportDiverRoundFlow>();

            var response = new ResReportDiverRoundFlow
            {

            };

            await WriteDataAsync(response);
        }
    }
}
