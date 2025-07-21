using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/picked/get")]
    public class GetPickedMessage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetPickedMessageList req = await ReadData<ReqGetPickedMessageList>();

            // TODO: get proper response
            ResGetPickedMessageList response = new();

            await WriteDataAsync(response);
        }
    }
}
