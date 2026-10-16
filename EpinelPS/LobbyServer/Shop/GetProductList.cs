using EpinelPS.Data;
using EpinelPS.Utils;
using System.Globalization; // Ensure this is included

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/inappshop/jupiter/getproductlist")]
public class GetProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetJupiterProductList x = await ReadData<ReqGetJupiterProductList>();

        ResGetJupiterProductList response = new();
        foreach (string? item in x.ProductIdList)
        {
            if (item == null) continue;
            if (GameData.Instance.JupiterProductCache.TryGetValue(item, out NetJupiterProductInfo? cached))
            {
                response.ProductInfoList.Add(cached);
            }
            else
            {
                Console.WriteLine($"Missing!!!! {item}");
            }
        }
        await WriteDataAsync(response);
    }
}
