using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Campaign
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
