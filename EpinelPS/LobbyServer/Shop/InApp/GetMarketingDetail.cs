using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp
{
    [PacketPath("/inappshop/jupiter/getmarketingdetail")]
    public class GetMarketingDetail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetJupiterMarketingDetail req = await ReadData<ReqGetJupiterMarketingDetail>();
            ResGetJupiterMarketingDetail response = new()
            {
                MarketingDetail = "{}"
            };

            await WriteDataAsync(response);
        }
    }
}
