namespace EpinelPS.LobbyServer.Shop.PackageShop;

[GameRequest("/packageshop/campaign/get")]
public class PackageShopGetCampaignPackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetCampaignPackage req = await ReadData<ReqGetCampaignPackage>();

        ResGetCampaignPackage response = new();

        // TODO: Validate response from real server and pull info from user info
        await WriteDataAsync(response);
    }
}
