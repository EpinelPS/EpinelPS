using nksrv.Net;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/maintenancenotice")]
    public class GetMaintenanceNotice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new MaintenanceNoticeResponse();

            await WriteDataAsync(r);
        }
    }
}
