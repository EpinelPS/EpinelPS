using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/jupiter/buyproduct")]
public class BuyJupiterProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqBuyJupiterProduct req = await ReadData<ReqBuyJupiterProduct>();
        string referenceId = $"epinel-jupiter-{req.ProductId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        int requestPackageListId = req.ExtraData?.PackageListTableId ?? 0;
        int packageListId = PackagePurchaseResolver.ResolvePackageListId(req.ProductId, requestPackageListId);
        PackageListRecord? packageList = packageListId != 0 && GameData.Instance.PackageListTable.TryGetValue(packageListId, out PackageListRecord? found)
            ? found
            : null;

        ResBuyJupiterProduct response = new()
        {
            ReferenceId = referenceId,
            RedirectUrl = $"https://global-lobby.nikke-kr.com/inappshop/jupiter/success?reference_id={Uri.EscapeDataString(referenceId)}"
        };

        InAppPurchaseStore.Add(new PendingInAppPurchase(
            User.ID,
            req.ProductId,
            referenceId,
            requestPackageListId,
            packageListId,
            packageList?.PackageShopId ?? 0,
            DateTime.UtcNow));

        await WriteDataAsync(response);
    }
}
