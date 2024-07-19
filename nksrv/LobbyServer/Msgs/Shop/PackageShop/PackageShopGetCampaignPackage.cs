using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Shop.PackageShop
{
    [PacketPath("/packageshop/campaign/get")]
    public class PackageShopGetCampaignPackage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCampaignPackage>();

            var response = new ResGetCampaignPackage();

            // TODO: Validate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}
