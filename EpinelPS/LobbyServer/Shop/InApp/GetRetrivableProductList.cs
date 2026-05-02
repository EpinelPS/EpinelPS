namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getreceivableproductlist")]
public class GetRetrivableProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetInAppShopReceivableProductList x = await ReadData<ReqGetInAppShopReceivableProductList>();

        ResGetInAppShopReceivableProductList response = new();
        // TODO

        await WriteDataAsync(response);
    }
}
