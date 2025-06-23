using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/productoffer/list")]
    public class GetProductOffers : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqListSeenProductOffer>();

            // Disable in game ads
            var response = new ResListSeenProductOffer();
            foreach(var item in GameData.Instance.ProductOffers)
            {
                response.Result.Add(new NetUserProductOfferSeenHistory() { ProductOfferId = item.Key });
            }

            await WriteDataAsync(response);
        }
    }
}
