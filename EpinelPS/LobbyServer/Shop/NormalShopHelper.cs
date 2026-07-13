using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Shop;

public static class NormalShopHelper
{
    private static readonly ILog log = LogManager.GetLogger(typeof(NormalShopHelper));

    public static List<NetShopProductData> GetAllShopData(User user)
    {
        var shops = new List<NetShopProductData>();
        var entries = GameData.Instance.ContentsShopTable.Values
            .Where(s => s.ShopCategory == ShopCategoryType.ShopNormal || s.ShopType == ShopType.MainShop);

        foreach (var entry in entries)
            shops.Add(BuildShopData(user, entry));

        return shops;
    }

    public static NetShopProductData GetShopData(User user, int shopCategory)
    {
        var entry = GameData.Instance.ContentsShopTable.Values
            .FirstOrDefault(s => (int)s.ShopCategory == shopCategory);

        return entry == null ? new NetShopProductData { ShopCategory = shopCategory } : BuildShopData(user, entry);
    }

    private static NetShopProductData BuildShopData(User user, ContentsShopRecord shopEntry)
    {
        var shop = new NetShopProductData
        {
            ShopTid = shopEntry.Id,
            ShopCategory = (int)shopEntry.ShopCategory,
            RenewCount = user.NormalShopBuyCountInfo.RenewCount,
            NextRenewAt = user.NormalShopBuyCountInfo.RenewAt,
        };
        var userBuyCounts = user.NormalShopBuyCountInfo.ProductBuyCounts;

        foreach (var csp in GameData.Instance.ContentsShopProductTable.Values.Where(csp => csp.BundleId == shopEntry.BundleId))
        {
            var buyCount = userBuyCounts.FirstOrDefault(x => x.ProductTid == csp.Id)?.BuyCount ?? 0;
            shop.List.Add(new NetShopProductInfoData
            {
                Order = csp.ProductOrder,
                ProductId = csp.Id,
                BuyLimitCount = csp.BuyLimitCount,
                BuyCount = buyCount,
            });
        }
        return shop;
    }

    public static void BuyShopProduct(User user, ref ResShopBuyProduct response, ReqShopBuyProduct req)
    {
        var buyProducts = new List<NetBuyProductRequestData>
        {
            new() { ShopProductTid = req.ShopProductTid, Quantity = req.Quantity, Order = req.Order }
        };

        var productData = new NetShopBuyMultipleProductData();
        var currencyChanges = new List<NetUserCurrencyData>();
        var itemChanges = new List<NetUserItemData>();

        if (!TryBuyProducts(user, productData, currencyChanges, itemChanges, buyProducts)) return;

        AddBuyCount(user, req.ShopProductTid, req.Quantity, req.Order, productData);

        response.Currencies.AddRange(currencyChanges);
        if (itemChanges.Count > 0) response.Item = itemChanges[0];

        response.Product = new NetShopBuyProductData();
        if (productData.UserItems.Count > 0) response.Product.UserItems.AddRange(productData.UserItems);
        if (productData.Item.Count > 0) response.Product.Item.AddRange(productData.Item);
        if (productData.Currency.Count > 0) response.Product.Currency.AddRange(productData.Currency);
        if (productData.BuyCounts.Count > 0) response.Product.BuyCount = productData.BuyCounts[0].BuyCount;
        if (productData.Character.Count > 0) response.Product.Character.AddRange(productData.Character);
        if (productData.UserCharacters.Count > 0) response.Product.UserCharacters.AddRange(productData.UserCharacters);
        if (productData.AutoCharge.Count > 0) response.Product.AutoCharge.AddRange(productData.AutoCharge);

        response.Result = ShopBuyProductResult.Success;
        JsonDb.Save();
    }

    public static void BuyShopMultipleProduct(User user, ref ResShopBuyMultipleProduct response, ReqShopBuyMultipleProduct req)
    {
        var buyProducts = req.Products.ToList();
        var currencyChanges = new List<NetUserCurrencyData>();
        var itemChanges = new List<NetUserItemData>();

        if (!TryBuyProducts(user, response.Product, currencyChanges, itemChanges, buyProducts)) return;

        foreach (var item in buyProducts)
            AddBuyCount(user, item.ShopProductTid, item.Quantity, item.Order, response.Product);

        response.Currencies.AddRange(currencyChanges);
        response.Items.AddRange(itemChanges);
        response.Result = ShopBuyProductResult.Success;
        JsonDb.Save();
    }

    public static void RenewShop(User user, ref ResShopRenew response, int shopCategory)
    {
        var entry = GameData.Instance.ContentsShopTable.Values
            .FirstOrDefault(s => (int)s.ShopCategory == shopCategory);

        if (entry == null) return;

        user.NormalShopBuyCountInfo.ProductBuyCounts.Clear();
        user.NormalShopBuyCountInfo.RenewCount++;
        user.NormalShopBuyCountInfo.RenewAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        response.Shop = BuildShopData(user, entry);
        JsonDb.Save();
    }

    private static bool TryBuyProducts(User user, NetShopBuyMultipleProductData productData,
        List<NetUserCurrencyData> currencyChanges, List<NetUserItemData> itemChanges,
        List<NetBuyProductRequestData> buyProducts)
    {
        if (buyProducts.Count == 0) return false;

        var productTids = buyProducts.Select(p => p.ShopProductTid).ToList();
        var shopProducts = GameData.Instance.ContentsShopProductTable.Values.Where(x => productTids.Contains(x.Id)).ToList();

        if (!CheckUserBalance(user, shopProducts, buyProducts, out var totalCurrencyPrice, out var totalItemPrice))
            return false;

        DeductUserCosts(user, totalCurrencyPrice, totalItemPrice, currencyChanges, itemChanges);

        foreach (var shopProduct in shopProducts)
        {
            var buyProduct = buyProducts.FirstOrDefault(bp => bp.ShopProductTid == shopProduct.Id);
            int qty = buyProduct?.Quantity ?? 1;
            int order = buyProduct?.Order ?? 0;

            if (shopProduct.GoodsType == RewardType.Item || shopProduct.GoodsType.ToString().StartsWith("Equipment"))
            {
                AddItem(user, ref productData, shopProduct.GoodsId, shopProduct.GoodsValue, qty, order);
            }
            else if (shopProduct.GoodsType == RewardType.Currency)
            {
                long val = (long)shopProduct.GoodsValue * qty;
                user.AddCurrency((CurrencyType)shopProduct.GoodsId, val);
                productData.Currency.Add(new NetCurrencyData
                {
                    Type = shopProduct.GoodsId,
                    Value = val,
                    FinalValue = user.GetCurrencyVal((CurrencyType)shopProduct.GoodsId)
                });
            }
            else if (shopProduct.GoodsType == RewardType.Character)
            {
                AddCharacter(user, ref productData, shopProduct.GoodsId, shopProduct.GoodsValue, qty, order);
            }
            else if (shopProduct.GoodsType == RewardType.UserTitle)
            {
                productData.UserTitleList.Add(shopProduct.GoodsId);
            }
            else if (shopProduct.GoodsType == RewardType.LiveWallpaper)
            {
                productData.LiveWallPapers.Add(shopProduct.GoodsId);
            }
            else
            {
                Logging.WriteLine($"Unsupported GoodsType in normal shop: {shopProduct.GoodsType}");
            }
        }
        return true;
    }

    private static void AddBuyCount(User user, int productTid, int quantity, int order, NetShopBuyMultipleProductData productData)
    {
        var buyCounts = user.NormalShopBuyCountInfo.ProductBuyCounts;
        var existing = buyCounts.FirstOrDefault(x => x.ProductTid == productTid);
        if (existing != null)
        {
            existing.BuyCount += quantity;
            productData.BuyCounts.Add(new NetBuyCountData { Order = order, BuyCount = existing.BuyCount });
        }
        else
        {
            buyCounts.Add(new EventShopProductData { ProductTid = productTid, BuyCount = quantity });
            productData.BuyCounts.Add(new NetBuyCountData { Order = order, BuyCount = quantity });
        }
    }

    private static bool CheckUserBalance(User user, List<ContentsShopProductRecord> shopProducts,
        List<NetBuyProductRequestData> buyProducts,
        out Dictionary<int, int> totalCurrencyPrices, out Dictionary<int, int> totalItemPrices)
    {
        totalCurrencyPrices = shopProducts
            .Where(sp => sp.PriceType == PriceType.Currency)
            .GroupBy(sp => sp.PriceId)
            .ToDictionary(g => g.Key, g => g.Sum(sp => sp.PriceValue * (buyProducts.FirstOrDefault(bp => bp.ShopProductTid == sp.Id)?.Quantity ?? 1)));

        totalItemPrices = shopProducts
            .Where(sp => sp.PriceType == PriceType.Item)
            .GroupBy(sp => sp.PriceId)
            .ToDictionary(g => g.Key, g => g.Sum(sp => sp.PriceValue * (buyProducts.FirstOrDefault(bp => bp.ShopProductTid == sp.Id)?.Quantity ?? 1)));

        foreach (int currencyType in totalCurrencyPrices.Keys)
        {
            var userCurrency = user.Currency.FirstOrDefault(x => x.Key == (CurrencyType)currencyType).Value;
            if (userCurrency < totalCurrencyPrices[currencyType])
            {
                Logging.WriteLine($"Normal shop: Insufficient funds: Have {userCurrency}, need {totalCurrencyPrices[currencyType]}");
                return false;
            }
        }
        foreach (int tid in totalItemPrices.Keys)
        {
            var item = user.Items.FirstOrDefault(i => i.ItemType == tid);
            if (item == null || item.Count < totalItemPrices[tid])
            {
                Logging.WriteLine($"Normal shop: Insufficient items: Have {item?.Count ?? 0}, need {totalItemPrices[tid]}");
                return false;
            }
        }
        return true;
    }

    private static void DeductUserCosts(User user, Dictionary<int, int> totalCurrencyPrice,
        Dictionary<int, int> totalItemPrice, List<NetUserCurrencyData> currencyChanges, List<NetUserItemData> itemChanges)
    {
        foreach (int key in totalCurrencyPrice.Keys)
        {
            var currencyType = (CurrencyType)key;
            user.SubtractCurrency(currencyType, totalCurrencyPrice[key]);
            currencyChanges.Add(new NetUserCurrencyData { Type = key, Value = user.GetCurrencyVal(currencyType) });
        }
        foreach (int tid in totalItemPrice.Keys)
        {
            var item = user.Items.FirstOrDefault(i => i.ItemType == tid);
            if (item != null)
            {
                user.RemoveItemBySerialNumber(item.Isn, totalItemPrice[tid]);
                itemChanges.Add(new NetUserItemData
                {
                    Tid = tid,
                    Count = user.Items.FirstOrDefault(i => i.ItemType == tid)?.Count ?? 0,
                    Isn = item.Isn
                });
            }
        }
    }

    private static void AddItem(User user, ref NetShopBuyMultipleProductData productData, int itemId, int goodsValue, int quantity, int order)
    {
        var userItemIndex = user.Items.FindIndex(i => i.ItemType == itemId);
        var isEquip = GameData.Instance.ItemEquipTable.TryGetValue(itemId, out var equip);

        if (userItemIndex >= 0)
        {
            if (isEquip)
            {
                for (int i = 0; i < goodsValue * quantity; i++)
                {
                    var (tid, pos, isn) = (itemId, GetItemPos(equip.ItemSubType), user.GenerateUniqueItemId());
                    var newItem = new DbItemData { ItemType = tid, Count = 1, Position = pos, Isn = isn, Corp = 0 };
                    user.Items.Add(newItem);
                    productData.Item.Add(NetUtils.ItemDataToNet(newItem));
                    productData.UserItems.Add(NetUtils.UserItemDataToNet(newItem));
                }
            }
            else
            {
                user.Items[userItemIndex].Count += goodsValue * quantity;
                productData.UserItems.Add(NetUtils.UserItemDataToNet(user.Items[userItemIndex]));
                productData.Item.Add(new NetItemData
                {
                    Tid = itemId,
                    Count = goodsValue * quantity,
                    Isn = user.Items[userItemIndex].Isn
                });
            }
        }
        else
        {
            var (tid, count, isn) = (itemId, goodsValue * quantity, user.GenerateUniqueItemId());
            var itemData = new DbItemData { ItemType = tid, Count = count, Isn = isn };
            user.Items.Add(itemData);
            productData.UserItems.Add(NetUtils.UserItemDataToNet(itemData));
            productData.Item.Add(NetUtils.ItemDataToNet(itemData));
        }
    }

    private static void AddCharacter(User user, ref NetShopBuyMultipleProductData productData, int characterTid, int goodsValue, int quantity, int order)
    {
        if (!GameData.Instance.CharacterTable.TryGetValue(characterTid, out var characterRecord))
            return;

        var userCharacter = user.GetCharacter(characterTid);
        bool isNew = userCharacter == null;

        if (isNew)
        {
            userCharacter = new CharacterModel
            {
                Csn = user.GenerateUniqueCharacterId(),
                Grade = 1,
                Tid = characterRecord.Id,
            };
            user.Characters.Add(userCharacter);
            productData.UserCharacters.Add(ToNetUserCharacter(userCharacter));
        }

        var netCharacter = new NetCharacterData { Csn = userCharacter.Csn, Tid = userCharacter.Tid };
        int materialNum = isNew ? goodsValue * quantity - 1 : goodsValue * quantity;

        if (materialNum > 0)
        {
            var rare = characterRecord.OriginalRare;
            int maxCore = rare == OriginalRareType.SSR ? 11 : rare == OriginalRareType.SR ? 3 : 1;
            int curCore = rare == OriginalRareType.SSR ? userCharacter.Grade : rare == OriginalRareType.SR ? userCharacter.Grade % 100 : 1;
            if (curCore > maxCore) curCore = maxCore;

            int curMat = user.Items.FirstOrDefault(x => x.ItemType == characterRecord.PieceId)?.Count ?? 0;
            int addMat = materialNum - curMat;
            bool capped = curCore + addMat > maxCore;

            if (capped)
            {
                int rate = rare == OriginalRareType.SSR ? 6000 : rare == OriginalRareType.SR ? 200 : 150;
                int overflow = (curCore + addMat - maxCore) * rate;
                addMat = maxCore - curCore;
                if (overflow > 0)
                {
                    netCharacter.CurrencyValue = overflow;
                    user.AddCurrency(CurrencyType.DissolutionPoint, overflow);
                }
            }

            if (addMat > 0)
            {
                netCharacter.PieceCount = addMat;
                AddItem(user, ref productData, characterRecord.PieceId, goodsValue, addMat, order);
            }
            productData.Character.Add(netCharacter);
        }
    }

    private static int GetItemPos(ItemSubType subType)
    {
        return subType switch
        {
            ItemSubType.ModuleA => 0,
            ItemSubType.ModuleB => 1,
            ItemSubType.ModuleC => 2,
            ItemSubType.ModuleD => 3,
            _ => 0,
        };
    }

    private static NetUserCharacterDefaultData ToNetUserCharacter(CharacterModel character)
    {
        return new NetUserCharacterDefaultData
        {
            CostumeId = character.CostumeId,
            Csn = character.Csn,
            Grade = character.Grade,
            Lv = character.Level,
            UltiSkillLv = character.UltimateLevel,
            Skill1Lv = character.Skill1Lvl,
            Skill2Lv = character.Skill2Lvl,
            Tid = character.Tid,
        };
    }
}
