using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/daily/pick")]
    public class GetDailyMessage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqPickTodayDailyMessage>();

            // TODO: save these things
            var response = new ResPickTodayDailyMessage();

            await WriteDataAsync(response);
        }
    }
}
