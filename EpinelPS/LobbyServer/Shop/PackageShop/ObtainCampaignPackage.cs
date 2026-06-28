using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Shop.InApp;

namespace EpinelPS.LobbyServer.Shop.PackageShop;

[GameRequest("/packageshop/campaign/obtain")]
public class ObtainCampaignPackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainCampaignPackage req = await ReadData<ReqObtainCampaignPackage>();

        NetRewardData totalReward = PackageRewardBuilder.BuildCampaignReward(
            User,
            req.PackageGroupId,
            req.ObtainIds,
            "PackageShop/campaign/obtain");

        InAppPurchaseStateService.MarkCampaignRewardRows(
            User,
            req.PackageGroupId,
            req.ObtainIds,
            "PackageShop/campaign/obtain");

        JsonDb.Save();

        ResObtainCampaignPackage response = new()
        {
            Reward = totalReward
        };

        await WriteDataAsync(response);
    }
}
