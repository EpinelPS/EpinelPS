using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Shop
{
    [PacketPath("/event/shopbuyproduct")]
    public class BuyProduct : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // int EventId
            var req = await ReadData<ReqEventShopBuyProduct>();
            ResEventShopBuyProduct response = new();
            User user = GetUser();
            try
            {
                EventShopHelper.BuyShopProduct(user, ref response, req);
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Error buying shop product: {ex.Message}", LogType.Error);
            }

            await WriteDataAsync(response);
        }
    }
}
