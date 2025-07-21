using EpinelPS.Utils;
using EpinelPS.Data; // For GameData access

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/record/notice")]
    public class MarkNoticeRead : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqRecordNoticeLog req = await ReadData<ReqRecordNoticeLog>();
            ResRecordNoticeLog r = new();
            Database.User user = GetUser();

            // TODO

            await WriteDataAsync(r);
        }
    }
}
