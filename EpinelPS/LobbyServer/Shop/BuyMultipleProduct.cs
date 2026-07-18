using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/multiple-buy")]
public class BuyMultipleProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyMultipleProduct req = await ReadData<ReqShopBuyMultipleProduct>();
        User user = GetUser();

        string details = string.Join(", ", req.Products.Select(p => $"Tid={p.ShopProductTid},Order={p.Order},Qty={p.Quantity}"));
        Logging.WriteLine($"[Shop] /shop/multiple-buy called by user {user.Nickname}: ShopCategory={req.ShopCategory}, Products=[{details}]", LogType.Debug);

        ResShopBuyMultipleProduct response = new()
        {
            Product = new NetShopBuyMultipleProductData(),
        };

        var requests = req.Products
            .Select(x => new NormalShopHelper.PurchaseRequest(x.ShopProductTid, x.Order, x.Quantity > 0 ? x.Quantity : 1))
            .ToList();
        if (!NormalShopHelper.TryPurchase(user, req.ShopCategory, requests, out var purchased,
                out var currencies, out var costItems))
        {
            response.Result = ShopBuyProductResult.Expired;
            await WriteDataAsync(response);
            return;
        }

        response.Currencies.AddRange(currencies);
        response.Items.AddRange(costItems);
        foreach (var purchase in purchased)
        {
            response.Product.BuyCounts.Add(new NetBuyCountData
            {
                Order = purchase.Order,
                BuyCount = purchase.BuyCount,
            });
            GrantProduct(user, ref response, purchase.Product, purchase.Quantity);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }

    private void GrantProduct(User user, ref ResShopBuyMultipleProduct response, ContentsShopProductRecord product, int quantity)
    {
        int totalValue = product.GoodsValue * quantity;

        if (product.GoodsType == RewardType.Currency)
        {
            user.AddCurrency((CurrencyType)product.GoodsId, totalValue);
            response.Product.Currency.Add(new NetCurrencyData
            {
                Type = product.GoodsId,
                Value = totalValue,
                FinalValue = user.GetCurrencyVal((CurrencyType)product.GoodsId),
            });
        }
        else if (product.GoodsType == RewardType.Item || product.GoodsType.ToString().StartsWith("Equipment"))
        {
            var existing = user.Items.FirstOrDefault(i => i.ItemType == product.GoodsId);
            if (existing != null)
            {
                existing.Count += totalValue;
                response.Product.Item.Add(NetUtils.ItemDataToNet(existing));
                response.Product.UserItems.Add(NetUtils.UserItemDataToNet(existing));
            }
            else
            {
                var newItem = new DbItemData
                {
                    ItemType = product.GoodsId,
                    Count = totalValue,
                    Isn = user.GenerateUniqueItemId(),
                };
                user.Items.Add(newItem);
                response.Product.Item.Add(NetUtils.ItemDataToNet(newItem));
                response.Product.UserItems.Add(NetUtils.UserItemDataToNet(newItem));
            }
        }
        else if (product.GoodsType == RewardType.Character)
        {
            if (GameData.Instance.CharacterTable.TryGetValue(product.GoodsId, out var charRecord))
            {
                var userChar = user.GetCharacter(product.GoodsId);
                if (userChar == null)
                {
                    userChar = new CharacterModel
                    {
                        Csn = user.GenerateUniqueCharacterId(),
                        Grade = 1,
                        Tid = charRecord.Id,
                    };
                    user.Characters.Add(userChar);
                }
                response.Product.Character.Add(new NetCharacterData
                {
                    Csn = userChar.Csn,
                    Tid = userChar.Tid,
                    PieceCount = totalValue,
                });
            }
        }
        else if (product.GoodsType == RewardType.LiveWallpaper)
        {
            if (!user.LiveWallpaperList.Contains(product.GoodsId))
                user.LiveWallpaperList = [.. user.LiveWallpaperList, product.GoodsId];
            response.Product.LiveWallPapers.Add(product.GoodsId);
        }
        else if (product.GoodsType == RewardType.UserTitle)
        {
            if (!user.TitleList.Contains(product.GoodsId))
                user.TitleList.Add(product.GoodsId);
            response.Product.UserTitleList.Add(product.GoodsId);
        }
        else
        {
            Logging.WriteLine($"[Shop] Unsupported reward type {product.GoodsType} for product {product.Id}", LogType.Warning);
        }
    }
}
