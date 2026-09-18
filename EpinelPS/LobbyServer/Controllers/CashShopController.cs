using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Services;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for user data
/// </summary>
[ApiController]
public class CashShopController : Controller
{
    [Route("/v1/inappshop/jupiter/getproductlist")]
    [HttpPost]
    public ActionResult<ResGetJupiterProductList> GetUserTitle([FromBodyProtobuf] ReqGetJupiterProductList req)
    {
        ResGetJupiterProductList response = new();
        foreach (string? item in req.ProductIdList)
        {
            IEnumerable<KeyValuePair<string, MidasProductRecord>> product = GameData.Instance.mediasProductTable.Where(x => x.Key == item);

            if (product.Any())
            {
                MidasProductRecord? record = product.FirstOrDefault().Value;
                if (record != null)
                {
                    string normalizedCost = record.Cost.Replace(',', '.');

                    if (!decimal.TryParse(normalizedCost, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                    {
                        Logging.WriteLine($"Failed to parse '{record.Cost}' (normalized as '{normalizedCost}'). Cash shop will not work properly.", LogType.Error);
                        continue;
                    }

                    long microPrice = (long)(price * 1000000);
                    response.ProductInfoList.Add(new NetJupiterProductInfo
                    {
                        CurrencyCode = "USD",
                        CurrencySymbol = "$",
                        MicroPrice = microPrice,
                        Price = record.Cost,
                        ProductId = item
                    });
                }
            }
            else
            {
                Console.WriteLine($"Missing!!!! {item}");
            }
        }

        return response;
    }
}
