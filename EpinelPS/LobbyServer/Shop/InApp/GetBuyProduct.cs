using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getbuyproduct")]
public class GetBuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetInAppShopBuyProduct req = await ReadData<ReqGetInAppShopBuyProduct>();

        bool claimedPendingPurchase = InAppPurchaseStore.TryClaim(User.ID, req.ProductId, req.Token, out PendingInAppPurchase? purchase);
        int packageListId = claimedPendingPurchase
            ? purchase!.PackageListId
            : PackagePurchaseResolver.ResolvePackageListId(req.ProductId);
        PackageListRecord? packageList = packageListId != 0 && GameData.Instance.PackageListTable.TryGetValue(packageListId, out PackageListRecord? found)
            ? found
            : null;

        NetRewardData reward = PackageRewardBuilder.BuildPurchaseReward(
            User,
            req.ProductId,
            claimedPendingPurchase ? purchase!.PackageShopId : packageList?.PackageShopId ?? 0,
            packageListId,
            "InAppShop/getbuyproduct");

        InAppPurchaseStateService.ApplySuccessfulPurchase(User, req.ProductId, "InAppShop/getbuyproduct");

        JsonDb.Save();

        ResGetInAppShopBuyProduct response = new()
        {
            Reward = reward
        };

        await WriteDataAsync(response);
    }
}
