using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/productoffer/list")]
public class GetProductOffers : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListSeenProductOffer x = await ReadData<ReqListSeenProductOffer>();

        // Disable in game ads
        ResListSeenProductOffer response = new();
        foreach (KeyValuePair<int, ProductOfferRecord> item in GameData.Instance.ProductOffers)
        {
            response.Result.Add(new NetUserProductOfferSeenHistory() { ProductOfferId = item.Key });
        }

        await WriteDataAsync(response);
    }
}
