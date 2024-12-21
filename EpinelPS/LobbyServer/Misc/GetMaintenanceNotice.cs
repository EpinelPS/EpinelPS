using EpinelPS.Utils;
//idk why i did this but it still works as long as IsWhitelisted is set to true
namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/maintenancenotice")]
    public class GetMaintenanceNotice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqMaintenanceNotice>(); // field string OpenId
            var oid = req.OpenId;

            // Create a new instance of ResMaintenanceNotice
            var r = new ResMaintenanceNotice
            {
                IsWhitelisted = true
            };

            // Define maintenance window timestamps
            var maintenanceFrom = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddHours(-2)); // Example: 2 hour ago
            var maintenanceTo = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddHours(-1));   // Example: 1 hour ago

            // Add a new maintenance window
            r.MaintenanceWindow = new NetMaintenanceWindow
            {
                From = maintenanceFrom,
                To = maintenanceTo
            };

            await WriteDataAsync(r);
        }
    }
}
