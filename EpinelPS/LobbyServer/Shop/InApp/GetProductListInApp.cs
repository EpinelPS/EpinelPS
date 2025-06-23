using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp
{
    [PacketPath("/inappshop/getdata")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetInAppShopData>();

            var response = new ResGetInAppShopData();

            response.InAppShopDataList.Add(new NetInAppShopData() { Id = 10001, StartDate = DateTime.Now.Ticks, EndDate = DateTime.Now.AddDays(2).Ticks });

            await WriteDataAsync(response);
        }
    }
}
