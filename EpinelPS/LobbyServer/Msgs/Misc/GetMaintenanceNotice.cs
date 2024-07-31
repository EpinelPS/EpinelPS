using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Misc
{
    [PacketPath("/maintenancenotice")]
    public class GetMaintenanceNotice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new ResMaintenanceNotice();

            await WriteDataAsync(r);
        }
    }
}
