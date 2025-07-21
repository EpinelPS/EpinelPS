using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/productoffer/list")]
    public class GetProductOffers : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListSeenProductOffer x = await ReadData<ReqListSeenProductOffer>();

            // Disable in game ads
            ResListSeenProductOffer response = new();
            foreach(KeyValuePair<int, ProductOfferRecord> item in GameData.Instance.ProductOffers)
            {
                response.Result.Add(new NetUserProductOfferSeenHistory() { ProductOfferId = item.Key });
            }

            await WriteDataAsync(response);
        }
    }
}
