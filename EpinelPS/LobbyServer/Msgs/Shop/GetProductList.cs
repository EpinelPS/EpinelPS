using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop
{
    [PacketPath("/inappshop/jupiter/getproductlist")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetJupiterProductList>();

            var response = new ResGetJupiterProductList();
            foreach (var item in x.ProductIdList)
            {
                // TODO: Optimize this!
                var product = GameData.Instance.mediasProductTable.Where(x => x.Key == item);

                if (product.Any())
                {
                    // Example:
                    // Midas RequestGetLocalPriceAsync res ProductId = com.proximabeta.nikke.costumegacha11_02, Price = 3.99, MicroPrice = 3990000, CurrencyCode = USD, CurrencySymbol = $
                    MidasProductRecord? record = product.FirstOrDefault().Value;
                    if (record != null)
                    {
                        if(!decimal.TryParse(record.cost, out decimal price))
                        {
                            Console.WriteLine("Failed to parse " + record.cost+" Cash shop will not work probably");
                        }

                        long microPrice = (long)(price * 1000000);
                        response.ProductInfoList.Add(new NetJupiterProductInfo() { CurrencyCode = "USD", CurrencySymbol = "$", MicroPrice = microPrice, Price = record.cost, ProductId = item });
                    }
                }
                else
                {
                    Console.WriteLine("Missing!!!! " + item);
                }
            }
            await WriteDataAsync(response);
        }
    }
}
