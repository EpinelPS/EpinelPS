using System.Collections.Concurrent;

namespace EpinelPS.LobbyServer.Shop.InApp;

internal sealed record PendingInAppPurchase(
    ulong UserId,
    string ProductId,
    string Token,
    int RequestPackageListId,
    int PackageListId,
    int PackageShopId,
    DateTime CreatedAt);

internal static class InAppPurchaseStore
{
    private static readonly ConcurrentDictionary<string, PendingInAppPurchase> PendingPurchases = [];

    public static void Add(PendingInAppPurchase purchase)
    {
        PendingPurchases[BuildKey(purchase.UserId, purchase.Token)] = purchase;
    }

    public static IReadOnlyList<PendingInAppPurchase> List(ulong userId)
    {
        DateTime cutoff = DateTime.UtcNow.AddHours(-6);

        foreach (KeyValuePair<string, PendingInAppPurchase> item in PendingPurchases)
        {
            if (item.Value.CreatedAt < cutoff)
            {
                PendingPurchases.TryRemove(item.Key, out _);
            }
        }

        return PendingPurchases.Values
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToList();
    }

    public static bool TryGet(string token, out PendingInAppPurchase? purchase)
    {
        purchase = PendingPurchases.Values.FirstOrDefault(item => item.Token == token);
        return purchase != null;
    }

    public static bool TryClaim(ulong userId, string productId, string token, out PendingInAppPurchase? purchase)
    {
        string key = BuildKey(userId, token);
        if (PendingPurchases.TryGetValue(key, out purchase) &&
            (string.IsNullOrWhiteSpace(productId) || purchase.ProductId == productId))
        {
            return PendingPurchases.TryRemove(key, out purchase);
        }

        purchase = null;
        return false;
    }

    private static string BuildKey(ulong userId, string token)
    {
        return $"{userId}:{token}";
    }
}
