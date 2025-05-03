using EpinelPS.Utils;
using EpinelPS.Data; // For GameData access

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/record/notice")]
    public class MarkNoticeRead : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRecordNoticeLog>();
            var r = new ResRecordNoticeLog();
            var user = GetUser();

            // TODO

            await WriteDataAsync(r);
        }
    }
}
