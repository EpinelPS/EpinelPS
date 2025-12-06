using EpinelPS.Utils;
//Idk why i dId this but it still works as long as IsWhitelisted is set to true
namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/maintenancenotice")]
    public class GetMaintenanceNotice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqMaintenanceNotice req = await ReadData<ReqMaintenanceNotice>(); // field string OpenId
            string oId = req.OpenId;

            // Create a new instance of ResMaintenanceNotice
            ResMaintenanceNotice r = new()
            {
                IsWhitelisted = true
            };

            // Define maintenance window timestamps
            /*Google.Protobuf.WellKnownTypes.Timestamp maintenanceFrom = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddHours(-2)); // Example: 2 hour ago
            Google.Protobuf.WellKnownTypes.Timestamp maintenanceTo = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddHours(-1));   // Example: 1 hour ago

            // Add a new maintenance window
            r.MaintenanceWindow = new NetMaintenanceWindow
            {
                From = maintenanceFrom,
                To = maintenanceTo
            };*/

            await WriteDataAsync(r);
        }
    }
}
