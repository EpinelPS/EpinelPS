using System;
using System.Globalization; // Ensure this is included
using System.Linq;
using System.Threading.Tasks;
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
                var product = GameData.Instance.mediasProductTable.Where(x => x.Key == item);

                if (product.Any())
                {
                    MidasProductRecord? record = product.FirstOrDefault().Value;
                    if (record != null)
                    {
                        string normalizedCost = record.cost.Replace(',', '.');

                        if (!decimal.TryParse(normalizedCost, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        {
                            Console.WriteLine($"Failed to parse '{record.cost}' (normalized as '{normalizedCost}'). Cash shop will not work properly.");
                            continue;
                        }

                        long microPrice = (long)(price * 1000000);
                        response.ProductInfoList.Add(new NetJupiterProductInfo
                        {
                            CurrencyCode = "USD",
                            CurrencySymbol = "$",
                            MicroPrice = microPrice,
                            Price = record.cost,
                            ProductId = item
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"Missing!!!! {item}");
                }
            }
            await WriteDataAsync(response);
        }
    }
}
