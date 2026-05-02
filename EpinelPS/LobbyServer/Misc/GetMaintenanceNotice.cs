namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/maintenancenotice")]
public class GetMaintenanceNotice : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqMaintenanceNotice req = await ReadData<ReqMaintenanceNotice>(); // field string OpenId
        string oId = req.OpenId;

        // Create a new instance of ResMaintenanceNotice
        ResMaintenanceNotice r = new();

        // TODO: add a way to define maintenance in admin panel

        await WriteDataAsync(r);
    }
}
