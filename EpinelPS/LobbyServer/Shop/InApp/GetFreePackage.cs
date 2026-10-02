using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getfreepackage")]
public class GetFreePackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetFreePackage req = await ReadData<ReqGetFreePackage>();
        User user = GetUser();

        int packageShopId = req.PackageShopTid;
        int packageListId = req.PackageListTid;

        if (packageShopId == 0 && packageListId != 0 &&
            GameData.Instance.PackageListTable.TryGetValue(packageListId, out var packageList))
        {
            packageShopId = packageList.PackageShopId;
        }

        int trackingKey = packageListId != 0 ? packageListId : packageShopId;

        NetRewardData reward = new() { PassPoint = new NetPassPointData() };

        // Check if already claimed during this server session
        if (InAppPurchaseHelper.HasClaimedFreePackageThisSession(user.ID, trackingKey))
        {
            await WriteDataAsync(new ResGetFreePackage { Reward = reward });
            return;
        }

        if (GameData.Instance.PackageShopTable.TryGetValue(packageShopId, out var package))
        {
            InAppPurchaseHelper.GrantPackageGroup(user, package.PackageGroupId, ref reward);
            InAppPurchaseHelper.RecordFreePackageClaimThisSession(
                user.ID,
                trackingKey,
                req.ProductType != 0 ? req.ProductType : (int)ShopProductType.Package,
                packageShopId);
            JsonDb.Save();
        }

        await WriteDataAsync(new ResGetFreePackage
        {
            Reward = reward
        });
    }
}
