using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

public static class NormalShopHelper
{
    public sealed record PurchaseRequest(int ProductId, int Order, int Quantity);
    public sealed record PurchasedProduct(ContentsShopProductRecord Product, int Order, int Quantity, int BuyCount);

    public static List<NetShopProductData> GetAllShopData(User user)
    {
        var result = new List<NetShopProductData>();

        foreach (var shop in GameData.Instance.ContentsShopTable.Values
                     .Where(s => s.ShopType == ShopType.MainShop)
                     // Several records describe later unlock stages of the same category.
                     // The client stores one ShopCategoryData per ShopCategoryType.
                     .GroupBy(s => s.ShopCategory)
                     .Select(g => g.OrderBy(s => s.Id).First())
                     .OrderBy(s => s.Id))
        {
            var data = BuildContentsShopData(user, shop);
            if (data.List.Count > 0)
                result.Add(data);
        }

        return result;
    }

    public static NetShopProductData GetShopData(User user, int shopCategory, bool reroll = false)
    {
        var shop = GameData.Instance.ContentsShopTable.Values.FirstOrDefault(s =>
            s.ShopType == ShopType.MainShop && (int)s.ShopCategory == shopCategory);
        if (shop == null)
            return new NetShopProductData { ShopCategory = shopCategory };

        return BuildContentsShopData(user, shop, reroll);
    }

    private static NetShopProductData BuildContentsShopData(User user, ContentsShopRecord shop, bool reroll = false)
    {
        var now = DateTime.UtcNow;
        var category = (int)shop.ShopCategory;
        bool manualReroll = reroll;
        if (!user.NormalShopStates.TryGetValue(category, out var state))
        {
            state = new NormalShopState();
            user.NormalShopStates[category] = state;
            reroll = true;
        }

        bool automaticRenewal = state.NextRenewAt > 0 && state.NextRenewAt <= now.Ticks;
        if (reroll || automaticRenewal || state.ProductIdsByOrder.Count == 0)
        {
            RollProducts(shop, state, reroll && state.ProductIdsByOrder.Count > 0);
            state.BuyCountsByOrder.Clear();
            state.RenewAt = GetRenewAt(shop, now);
            state.NextRenewAt = GetNextRenewAt(shop, now);
            if (manualReroll && !automaticRenewal)
                state.RenewCount++;
        }

        var result = new NetShopProductData
        {
            ShopTid = shop.Id,
            ShopCategory = category,
            RenewCount = state.RenewCount,
            RenewAt = state.RenewAt,
            NextRenewAt = state.NextRenewAt,
            FreeRenewCount = Math.Max(0, 1 - state.RenewCount),
        };

        foreach (var selection in state.ProductIdsByOrder.OrderBy(x => x.Key))
        {
            if (!GameData.Instance.ContentsShopProductTable.TryGetValue(selection.Value, out var product))
                continue;

            result.List.Add(new NetShopProductInfoData
            {
                Order = product.ProductOrder,
                ProductId = product.Id,
                BuyLimitCount = product.IsLimit ? product.BuyLimitCount : 0,
                BuyCount = state.BuyCountsByOrder.GetValueOrDefault(product.ProductOrder),
            });
        }

        return result;
    }

    private static long GetRenewAt(ContentsShopRecord shop, DateTime now)
    {
        if (shop.RenewType == RenewType.None)
            return 0;

        return now.Date.Ticks;
    }

    private static long GetNextRenewAt(ContentsShopRecord shop, DateTime now)
    {
        // Renewal timestamps are part of the shop state. Returning `now +
        // period` makes every product-list response different and causes the
        // client to immediately request /shop/productlist again in a loop.
        // Use a stable UTC boundary instead.
        var days = shop.RenewType switch
        {
            RenewType.AutoDay => Math.Max(1, shop.RenewValue),
            RenewType.AutoWeek => Math.Max(1, shop.RenewValue) * 7,
            RenewType.AutoMonth => Math.Max(1, shop.RenewValue) * 30,
            _ => 0
        };

        return days == 0 ? 0 : now.Date.AddDays(days).Ticks;
    }

    public static bool TryGetSelectedProduct(User user, int shopCategory, int order, int productId,
        out ContentsShopProductRecord? product, out NormalShopState? state)
    {
        product = null;
        state = null;
        _ = GetShopData(user, shopCategory);

        if (!user.NormalShopStates.TryGetValue(shopCategory, out state))
            return false;

        // Some client versions send the list index instead of ProductOrder.
        // ProductId is authoritative because it is the value returned in the
        // current user's shop list.
        if (!state.ProductIdsByOrder.TryGetValue(order, out int selectedId) || selectedId != productId)
        {
            var selected = state.ProductIdsByOrder.FirstOrDefault(x => x.Value == productId);
            if (selected.Value != productId)
                return false;
        }

        return GameData.Instance.ContentsShopProductTable.TryGetValue(productId, out product);
    }

    public static void AddBuyCount(NormalShopState state, int order, int quantity)
    {
        state.BuyCountsByOrder[order] = state.BuyCountsByOrder.GetValueOrDefault(order) + quantity;
    }

    public static bool TryPurchase(User user, int shopCategory, IReadOnlyCollection<PurchaseRequest> requests,
        out List<PurchasedProduct> purchased, out List<NetUserCurrencyData> currencies, out List<NetUserItemData> items)
    {
        purchased = [];
        currencies = [];
        items = [];
        if (requests.Count == 0 || requests.Any(x => x.Quantity <= 0))
            return false;

        var resolved = new List<(PurchaseRequest Request, ContentsShopProductRecord Product, NormalShopState State)>();
        foreach (var request in requests)
        {
            if (!TryGetSelectedProduct(user, shopCategory, request.Order, request.ProductId, out var product, out var state) ||
                product == null || state == null)
                return false;

            int actualOrder = state.ProductIdsByOrder.First(x => x.Value == product.Id).Key;
            var normalizedRequest = request with { Order = actualOrder };
            int currentBuyCount = state.BuyCountsByOrder.GetValueOrDefault(actualOrder);
            if (product.IsLimit && currentBuyCount + normalizedRequest.Quantity > product.BuyLimitCount)
                return false;

            resolved.Add((normalizedRequest, product, state));
        }

        var currencyCosts = resolved
            .Where(x => x.Product.PriceType == PriceType.Currency)
            .GroupBy(x => x.Product.PriceId)
            .ToDictionary(x => x.Key, x => x.Sum(y => checked(y.Product.PriceValue * y.Request.Quantity)));
        var itemCosts = resolved
            .Where(x => x.Product.PriceType == PriceType.Item)
            .GroupBy(x => x.Product.PriceId)
            .ToDictionary(x => x.Key, x => x.Sum(y => checked(y.Product.PriceValue * y.Request.Quantity)));

        if (currencyCosts.Any(x => !user.CanSubtractCurrency((CurrencyType)x.Key, x.Value)))
            return false;
        if (itemCosts.Any(x => (user.Items.FirstOrDefault(i => i.ItemType == x.Key)?.Count ?? 0) < x.Value))
            return false;

        foreach (var cost in currencyCosts)
        {
            user.SubtractCurrency((CurrencyType)cost.Key, cost.Value);
            currencies.Add(new NetUserCurrencyData
            {
                Type = cost.Key,
                Value = user.GetCurrencyVal((CurrencyType)cost.Key),
            });
        }

        foreach (var cost in itemCosts)
        {
            var item = user.Items.First(i => i.ItemType == cost.Key);
            user.RemoveItemBySerialNumber(item.Isn, cost.Value);
            items.Add(new NetUserItemData { Tid = item.ItemType, Isn = item.Isn, Count = item.Count });
        }

        foreach (var entry in resolved)
        {
            AddBuyCount(entry.State, entry.Request.Order, entry.Request.Quantity);
            purchased.Add(new PurchasedProduct(entry.Product, entry.Request.Order, entry.Request.Quantity,
                entry.State.BuyCountsByOrder[entry.Request.Order]));
        }

        return true;
    }

    private static void RollProducts(ContentsShopRecord shop, NormalShopState state, bool avoidPrevious)
    {
        var previous = state.ProductIdsByOrder;
        var next = new Dictionary<int, int>();

        foreach (var group in GameData.Instance.ContentsShopProductTable.Values
                     .Where(p => p.BundleId == shop.BundleId)
                     .GroupBy(p => p.ProductOrder))
        {
            var candidates = group.ToList();
            if (avoidPrevious && candidates.Count > 1 && previous.TryGetValue(group.Key, out int previousId))
                candidates.RemoveAll(x => x.Id == previousId);

            int totalWeight = candidates.Sum(x => Math.Max(0, x.ProductProb));
            ContentsShopProductRecord selected;
            if (totalWeight == 0)
            {
                selected = candidates[Random.Shared.Next(candidates.Count)];
            }
            else
            {
                int roll = Random.Shared.Next(totalWeight);
                selected = candidates[0];
                foreach (var candidate in candidates)
                {
                    roll -= Math.Max(0, candidate.ProductProb);
                    if (roll < 0)
                    {
                        selected = candidate;
                        break;
                    }
                }
            }

            next[group.Key] = selected.Id;
        }

        state.ProductIdsByOrder = next;
    }
}
