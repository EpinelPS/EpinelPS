using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/shop/productlist")]
    public class GetShopProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqShopProductList req = await ReadData<ReqShopProductList>();
            ResShopProductList response = new();

            await WriteDataAsync(response);
        }
    }
}
