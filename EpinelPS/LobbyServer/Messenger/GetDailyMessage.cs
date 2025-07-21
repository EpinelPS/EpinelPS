using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/daily/pick")]
    public class GetDailyMessage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqPickTodayDailyMessage req = await ReadData<ReqPickTodayDailyMessage>();

            // TODO: save these things
            ResPickTodayDailyMessage response = new();

            await WriteDataAsync(response);
        }
    }
}
