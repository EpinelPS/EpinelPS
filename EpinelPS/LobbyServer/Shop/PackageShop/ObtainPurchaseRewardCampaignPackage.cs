using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Shop.InApp;

namespace EpinelPS.LobbyServer.Shop.PackageShop;

[GameRequest("/inappshop/campaignpackage/obtainpurchasereward")]
public class ObtainPurchaseRewardCampaignPackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainPurchaseRewardCampaignPackage req = await ReadData<ReqObtainPurchaseRewardCampaignPackage>();

        int packageGroupId = ResolvePackageGroupId(req.PackageId);
        List<int> purchaseRewardIds = GameData.Instance.CampaignPackageGroupTable.Values
            .Where(item => item.PackageGroupId == packageGroupId && item.RewardType == CampaignPackageRewardType.Purchase)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToList();

        NetRewardData reward = PackageRewardBuilder.BuildCampaignReward(
            User,
            packageGroupId,
            purchaseRewardIds,
            "PackageShop/campaign/obtainpurchasereward");

        InAppPurchaseStateService.MarkCampaignRewardRows(
            User,
            packageGroupId,
            purchaseRewardIds,
            "PackageShop/campaign/obtainpurchasereward");

        JsonDb.Save();

        ResObtainPurchaseRewardCampaignPackage response = new()
        {
            Reward = reward
        };

        await WriteDataAsync(response);
    }

    private static int ResolvePackageGroupId(int packageId)
    {
        return GameData.Instance.CampaignPackageShopTable.TryGetValue(packageId, out CampaignPackageShopRecord? campaignShop)
            ? campaignShop.PackageGroupId
            : packageId;
    }
}
