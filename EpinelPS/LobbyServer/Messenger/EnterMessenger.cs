using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/enter")]
    public class EnterMessenger : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterMessengerDialog>();

            // TODO: save these things
            var response = new ResEnterMessengerDialog();
            response.Message = new NetMessage(){
ConversationId = "m_mainafter_28_01",
CreatedAt = 132123123,
MessageId = "m_mainafter_28_01_1",
Seq = 324234,
State = 0
            };

            await WriteDataAsync(response);
        }
    }
}
