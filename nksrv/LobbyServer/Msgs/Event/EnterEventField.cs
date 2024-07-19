using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Event
{
    [PacketPath("/eventfield/enter")]
    public class EnterEventField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterEventField>();

            var response = new ResEnterEventField();

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
