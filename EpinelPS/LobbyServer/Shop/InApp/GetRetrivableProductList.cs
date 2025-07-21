using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp
{
    [PacketPath("/inappshop/getreceivableproductlist")]
    public class GetRetrivableProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetInAppShopReceivableProductList x = await ReadData<ReqGetInAppShopReceivableProductList>();

            ResGetInAppShopReceivableProductList response = new();
            // TODO

            await WriteDataAsync(response);
        }
    }
}
