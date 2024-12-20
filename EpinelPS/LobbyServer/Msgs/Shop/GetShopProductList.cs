using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop
{
    [PacketPath("/shop/productlist")]
    public class GetShopProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShopProductList>();
            var response = new ResShopProductList();

            response.Shops.Add(new NetShopProductData()
            {
               FreeRenewCount = 1,
               ShopTid =  1,
               ShopCategory = (int)ShopCategoryType.Normal,
               RenewAt = DateTime.Now.AddDays(1).Ticks,
               NextRenewAt = DateTime.Now.AddDays(1).Ticks,
               RenewCount = 0
            });

            response.Shops[0].List.Add(new NetShopProductInfoData() { BuyCount = 0, ProductId = 201, CorporationType = 0});

            await WriteDataAsync(response);
        }
    }
}
