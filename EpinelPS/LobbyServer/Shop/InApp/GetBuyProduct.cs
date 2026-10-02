namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getbuyproduct")]
public class GetBuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetInAppShopBuyProduct>();
        await WriteDataAsync(new ResGetInAppShopBuyProduct
        {
            Reward = new NetRewardData { IsEmptyReward = true, PassPoint = new NetPassPointData() },
            PurchasePointResult = new NetInAppShopPurchasePointAcquireResult(),
        });
    }
}
