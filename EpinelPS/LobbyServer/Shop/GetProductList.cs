using System;
using System.Globalization; // Ensure this is included
using System.Linq;
using System.Threading.Tasks;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/inappshop/jupiter/getproductlist")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetJupiterProductList x = await ReadData<ReqGetJupiterProductList>();

            ResGetJupiterProductList response = new();
            foreach (string? item in x.ProductIdList)
            {
                IEnumerable<KeyValuePair<string, MidasProductRecord>> product = GameData.Instance.mediasProductTable.Where(x => x.Key == item);

                if (product.Any())
                {
                    MidasProductRecord? record = product.FirstOrDefault().Value;
                    if (record != null)
                    {
                        string normalizedCost = record.cost.Replace(',', '.');

                        if (!decimal.TryParse(normalizedCost, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        {
                            Logging.WriteLine($"Failed to parse '{record.cost}' (normalized as '{normalizedCost}'). Cash shop will not work properly.", LogType.Error);
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
