namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getbuyproduct")]
public class GetBuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetInAppShopBuyProduct req = await ReadData<ReqGetInAppShopBuyProduct>();
        User user = GetUser();
        await WriteDataAsync(new ResGetInAppShopBuyProduct
        {
            Reward = InAppPurchaseHelper.TakePendingReward(user.ID, req.ProductId),
            PurchasePointResult = new NetInAppShopPurchasePointAcquireResult(),
        });
    }
}
