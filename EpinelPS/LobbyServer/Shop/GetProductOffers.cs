using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/productoffer/list")]
    public class GetProductOffers : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqListSeenProductOffer>();

            // TODO: Figure out a way to disable ads

            var response = new ResListSeenProductOffer();

            await WriteDataAsync(response);
        }
    }
}
