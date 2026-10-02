namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getreceivableproductlist")]
public class GetRetrivableProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetInAppShopReceivableProductList>();
        await WriteDataAsync(new ResGetInAppShopReceivableProductList());
    }
}
