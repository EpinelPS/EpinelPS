namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getdata")]
public class GetProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetInAppShopData x = await ReadData<ReqGetInAppShopData>();

        ResGetInAppShopData response = new();

        response.InAppShopDataList.Add(new NetInAppShopData() { Id = 10001, StartDate = DateTime.Now.Ticks, EndDate = DateTime.Now.AddDays(2).Ticks });

        await WriteDataAsync(response);
    }
}
