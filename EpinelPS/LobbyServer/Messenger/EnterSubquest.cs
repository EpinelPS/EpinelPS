using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/subquest/enter")]
    public class EnterSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterSubQuestMessengerDialog>();

            // TODO: save these things
            var response = new ResEnterSubQuestMessengerDialog();

            await WriteDataAsync(response);
        }
    }
}
