using EpinelPS.Net;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Misc
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
