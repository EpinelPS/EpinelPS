using EpinelPS.LobbyServer.Shop.InApp;

namespace EpinelPS.LobbyServer.Shop.PackageShop;

[GameRequest("/packageshop/campaign/get")]
public class PackageShopGetCampaignPackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetCampaignPackage>();

        ResGetCampaignPackage response = new();
        response.AlreadyRewards.AddRange(InAppPurchaseStateService.BuildCampaignAlreadyRewards(User));

        await WriteDataAsync(response);
    }
}
