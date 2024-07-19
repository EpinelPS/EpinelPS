using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/picked/get")]
    public class GetPickedMessage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetPickedMessageList>();

            // TODO: get proper response
            var response = new ResGetPickedMessageList();

            await WriteDataAsync(response);
        }
    }
}
