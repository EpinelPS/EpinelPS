using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Shop
{
    [PacketPath("/event/shopproductlist")]
    public class ListProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqShopProductList req = await ReadData<ReqShopProductList>();
            Database.User user = GetUser();

            ResShopProductList response = new();
            response.Shops.Add(new NetShopProductData()
            {

            });

            // TODO implement properly

            await WriteDataAsync(response);
        }
    }
}
