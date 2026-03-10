using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Shop
{
    [PacketPath("/event/shopproductlist")]
    public class ProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // int EventId
            var req = await ReadData<ReqEventShopProductList>();
            ResEventShopProductList response = new();
            User user = User;
            
            try
            {
                response.Shops.Add(EventShopHelper.InitShopData(user, req.EventId));
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Get EventShopProductList Error: {ex.Message}", LogType.Error);
            }

            await WriteDataAsync(response);
        }
    }
}
