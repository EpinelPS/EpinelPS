using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.PackageShop
{
    [PacketPath("/packageshop/campaign/get")]
    public class PackageShopGetCampaignPackage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetCampaignPackage req = await ReadData<ReqGetCampaignPackage>();

            ResGetCampaignPackage response = new();

            // TODO: Validate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}
