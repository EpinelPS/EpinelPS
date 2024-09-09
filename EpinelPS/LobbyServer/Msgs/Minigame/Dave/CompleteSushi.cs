using EpinelPS.Utils;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/completesushi")]
    public class CompleteDaveSushi : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCompleteDaveSushi>();

            var response = new ResCompleteDaveSushi
            {

            };

            await WriteDataAsync(response);
        }
    }
}
