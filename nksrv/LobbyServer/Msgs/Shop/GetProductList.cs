using nksrv.Utils;
using Swan.Logging;

namespace nksrv.LobbyServer.Msgs.Shop
{
    [PacketPath("/inappshop/jupiter/getproductlist")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            try
            {
                var x = await ReadData<ReqGetJupiterProductList>();

                var response = new ResGetJupiterProductList();
                foreach (var item in x.ProductIdList)
                {
                    response.ProductInfoList.Add(new NetJupiterProductInfo() { CurrencyCode = "US", CurrencySymbol = "$", MicroPrice = 0, Price = "1", ProductId = item });
                }
                await WriteDataAsync(response);
            }
            catch (Exception ex)
            {
                Logger.Error("Error while handling GetProductList request. Have you replaced sodium library?" + ex);
            }
        }
    }
}
