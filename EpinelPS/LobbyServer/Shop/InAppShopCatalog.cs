using System.Collections.Concurrent;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop;

internal static class InAppShopCatalog
{
    private static readonly ConcurrentDictionary<int, MidasProductRecord> SeenProducts = [];

    public static void Record(MidasProductRecord record)
    {
        if (record.Id == 0 || !record.IsActive || string.IsNullOrWhiteSpace(record.MidasProductIdProximabeta))
        {
            return;
        }

        SeenProducts.TryAdd(record.Id, record);
    }

    public static IReadOnlyList<int> BuildVisibleProductIds(int limit = 32)
    {
        List<int> result = [10001];

        result.AddRange(SeenProducts.Values
            .Where(record => record.Id != 10001)
            .OrderBy(record => record.Id)
            .Select(record => record.Id)
            .Take(limit));

        return result.Distinct().ToList();
    }
}
