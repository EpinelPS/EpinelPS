using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/shop/get")]
    public class GetShop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetShop x = await ReadData<ReqGetShop>();

            ResGetShop response = new()
            {
                Shop = new NetShopProductData
                {
                    ShopCategory = x.ShopCategory
                }
            };

            // TODO

            await WriteDataAsync(response);
        }
    }
}
