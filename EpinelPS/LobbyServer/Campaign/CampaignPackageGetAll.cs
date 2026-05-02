namespace EpinelPS.LobbyServer.Campaign;

[GameRequest("/shutdownflags/campaignpackage/getall")]
public class CampaignPackageGetAll : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCampaignPackageGetAllShutdownFlags req = await ReadData<ReqCampaignPackageGetAllShutdownFlags>();

        ResCampaignPackageGetAllShutdownFlags response = new();
        // TODO

        await WriteDataAsync(response);
    }
}
