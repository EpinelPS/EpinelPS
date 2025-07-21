using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/record/notice")]
    public class MarkNoticeRead : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqRecordNoticeLog req = await ReadData<ReqRecordNoticeLog>();
            ResRecordNoticeLog r = new();
            User user = GetUser();

            // TODO

            await WriteDataAsync(r);
        }
    }
}
