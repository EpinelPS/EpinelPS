using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/shop/productlist")]
    public class GetShopProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShopProductList>();
            var response = new ResShopProductList();

            await WriteDataAsync(response);
        }
    }
}
