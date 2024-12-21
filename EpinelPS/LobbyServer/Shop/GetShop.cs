using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/shop/get")]
    public class GetShop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetShop>();

            var response = new ResGetShop();

            response.Shop = new NetShopProductData();
            response.Shop.ShopCategory = x.ShopCategory;
        
            // TODO

            await WriteDataAsync(response);
        }
    }
}
