using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp
{
    [PacketPath("/inappshop/getdata")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetInAppShopData>();

            var response = new ResGetInAppShopData();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
