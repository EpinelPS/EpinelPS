namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/jupiter/getmarketingdetail")]
public class GetMarketingDetail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetJupiterMarketingDetail>();
        ResGetJupiterMarketingDetail response = new()
        {
            MarketingDetail = "{}"
        };

        await WriteDataAsync(response);
    }
}
