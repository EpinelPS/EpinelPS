using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign
{
    [PacketPath("/shutdownflags/campaignpackage/getall")]
    public class CampaignPackageGetAll : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCampaignPackageGetAllShutdownFlags req = await ReadData<ReqCampaignPackageGetAllShutdownFlags>();

            ResCampaignPackageGetAllShutdownFlags response = new();
            // TODO

            await WriteDataAsync(response);
        }
    }
}
