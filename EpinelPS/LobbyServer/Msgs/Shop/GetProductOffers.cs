using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop
{
    [PacketPath("/productoffer/list")]
    public class GetProductOffers : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ResListSeenProductOffer>();

            // TODO

            var response = new ReqListSeenProductOffer();
            await WriteDataAsync(response);
        }
    }
}
