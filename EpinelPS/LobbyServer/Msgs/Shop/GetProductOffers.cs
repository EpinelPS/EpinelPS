using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop
{
    [PacketPath("/productoffer/list")]
    public class GetProductOffers : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // TODO use proper type: ResListSeenProductOffer
            var x = await ReadData<ReqGetJupiterProductList>();

            var response = new ResGetJupiterProductList();
            await WriteDataAsync(response);
        }
    }
}
