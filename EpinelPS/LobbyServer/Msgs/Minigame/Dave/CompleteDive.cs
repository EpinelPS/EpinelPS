using EpinelPS.Utils;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/completedive")]
    public class CompleteDaveDive : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCompleteDaveDive>();

            var response = new ResCompleteDaveDive
            {
               
            };

            await WriteDataAsync(response);
        }
    }
}
