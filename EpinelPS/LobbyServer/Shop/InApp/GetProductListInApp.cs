namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getdata")]
public class GetProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetInAppShopData x = await ReadData<ReqGetInAppShopData>();

        ResGetInAppShopData response = new();

        // Keep the advertised period stable across repeated polling. Using
        // DateTime.Now here changes the payload on every request and makes the
        // client re-enter its shop refresh path continuously.
        var start = DateTime.UtcNow.Date;
        response.InAppShopDataList.Add(new NetInAppShopData
        {
            Id = 10001,
            StartDate = start.Ticks,
            EndDate = start.AddDays(2).Ticks,
        });

        await WriteDataAsync(response);
    }
}
