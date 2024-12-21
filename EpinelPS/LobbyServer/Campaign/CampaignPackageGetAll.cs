using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign
{
    [PacketPath("/shutdownflags/campaignpackage/getall")]
    public class CampaignPackageGetAll : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCampaignPackageGetAllShutdownFlags>();

            var response = new ResCampaignPackageGetAllShutdownFlags();
            // TODO

            await WriteDataAsync(response);
        }
    }
}
