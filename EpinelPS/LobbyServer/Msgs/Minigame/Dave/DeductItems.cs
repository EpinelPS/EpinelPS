using EpinelPS.Utils;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/deductitem")]
    public class DeductDaveItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqDeductDaveItem>();

            var response = new ResDeductDaveItem
            {

            };

            await WriteDataAsync(response);
        }
    }
}
