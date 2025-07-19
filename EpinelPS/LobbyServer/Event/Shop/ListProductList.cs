using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Shop
{
    [PacketPath("/event/shopproductlist")]
    public class ListProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShopProductList>();
            var user = GetUser();

            var response = new ResShopProductList();
            response.Shops.Add(new NetShopProductData()
            {

            });

            // TODO implement properly

            await WriteDataAsync(response);
        }
    }
}
