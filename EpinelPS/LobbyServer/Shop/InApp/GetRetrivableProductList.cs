namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getreceivableproductlist")]
public class GetRetrivableProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetInAppShopReceivableProductList>();

        ResGetInAppShopReceivableProductList response = new();
        IReadOnlyList<PendingInAppPurchase> pendingPurchases = InAppPurchaseStore.List(User.ID);

        foreach (PendingInAppPurchase purchase in pendingPurchases)
        {
            response.DataList.Add(new NetInAppShopReceivableProductData()
            {
                ProductId = purchase.ProductId,
                Token = purchase.Token,
                SubTid = purchase.RequestPackageListId != 0 ? purchase.RequestPackageListId : purchase.PackageListId
            });
        }

        await WriteDataAsync(response);
    }
}
