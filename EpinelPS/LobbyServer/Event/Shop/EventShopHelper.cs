using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Event.Shop;

public static class EventShopHelper
{
    private static readonly ILog log = LogManager.GetLogger(typeof(EventShopHelper));


    public static void BuyShopProduct(User user, ref ResEventShopBuyProduct response, ReqEventShopBuyProduct req)
    {
        ResEventShopMultipleBuyProduct MultipleResponse = new();
        List<NetBuyProductRequestData> buyProducts = [new() { ShopProductTid = req.ShopProductTid, Quantity = req.Quantity, Order = req.Order }];
        bool isSuccess = ExecuteBuyProduct(user, ref MultipleResponse, buyProducts);

        if (!isSuccess) return;
        AddEventShopBuyCount(user, ref MultipleResponse, req.EventId, req.ShopProductTid, req.Quantity, req.Order);
        JsonDb.Save();

        // Update currency data
        if (MultipleResponse.Currencies.Count > 0)
            response.Currencies.AddRange(MultipleResponse.Currencies);

        // Update item data
        if (MultipleResponse.Items.Count > 0)
            response.Item = MultipleResponse.Items[0];

        // Update product data
        response.Product = new();
        if (MultipleResponse.Product.UserItems.Count > 0) response.Product.UserItems.AddRange(MultipleResponse.Product.UserItems);

        if (MultipleResponse.Product.Item.Count > 0) response.Product.Item.AddRange(MultipleResponse.Product.Item);

        if (MultipleResponse.Product.Currency.Count > 0) response.Product.Currency.AddRange(MultipleResponse.Product.Currency);

        if (MultipleResponse.Product.BuyCounts.Count > 0) response.Product.BuyCount = MultipleResponse.Product.BuyCounts[0].BuyCount;

        if (MultipleResponse.Product.Character.Count > 0) response.Product.Character.AddRange(MultipleResponse.Product.Character);

        if (MultipleResponse.Product.UserCharacters.Count > 0) response.Product.UserCharacters.AddRange(MultipleResponse.Product.UserCharacters);

        if (MultipleResponse.Product.AutoCharge.Count > 0) response.Product.AutoCharge.AddRange(MultipleResponse.Product.AutoCharge);

        // user.AddTrigger(Trigger.MainShopBuy, req.Quantity);

        // Save changes to the database
        JsonDb.Save();
    }

    public static void BuyShopMultipleProduct(User user, ref ResEventShopMultipleBuyProduct response, ReqEventShopMultipleBuyProduct req)
    {
        bool isSuccess = ExecuteBuyProduct(user, ref response, [.. req.Products]);

        if (!isSuccess) return;
        foreach (var item in req.Products)
        {
            AddEventShopBuyCount(user, ref response, req.EventId, item.ShopProductTid, item.Quantity, item.Order);
        }
        JsonDb.Save();
    }

    private static void AddEventShopBuyCount(User user, ref ResEventShopMultipleBuyProduct response, int eventId, int productTid, int quantity, int order)
    {
        if (!user.EventShopBuyCountInfo.TryGetValue(eventId, out var buyCountInfo))
        {
            buyCountInfo = new() { EventId = eventId, datas = [] };
            user.EventShopBuyCountInfo.TryAdd(eventId, buyCountInfo);
        }

        var index = buyCountInfo.datas.FindIndex(x => x.ProductTid == productTid);
        if (index >= 0)
        {
            buyCountInfo.datas[index].BuyCount += quantity;
            response.Product.BuyCounts.Add(new NetBuyCountData { Order = order, BuyCount = buyCountInfo.datas[index].BuyCount });
        }
        else
        {
            buyCountInfo.datas.Add(new() { ProductTid = productTid, BuyCount = quantity });
            response.Product.BuyCounts.Add(new NetBuyCountData { Order = order, BuyCount = quantity });
        }
        user.EventShopBuyCountInfo[eventId] = buyCountInfo;
    }

    private static bool ExecuteBuyProduct(User user, ref ResEventShopMultipleBuyProduct response, List<NetBuyProductRequestData> buyProducts)
    {
        if (buyProducts == null || buyProducts.Count == 0) return false;

        response.Product = new();

        var productTids = buyProducts.Select(p => p.ShopProductTid).ToList();
        var shopProducts = GameData.Instance.ContentsShopProductTable.Values.Where(x => productTids.Contains(x.Id)).ToList();

        // Check user currency and item balance
        if (CheckUserCurrencyAndItemBalance(user, shopProducts, buyProducts, out Dictionary<int, int> totalCurrencyPrice, out Dictionary<int, int> totalItemPrice))
        {
            // Deduct user currency and item
            DeductUserCurrencyAndItems(user, totalCurrencyPrice, totalItemPrice, ref response);
        }
        else
        {
            return false;
        }

        // Process each shopProduct
        foreach (var shopProduct in shopProducts)
        {
            var buyProduct = buyProducts.FirstOrDefault(bp => bp.ShopProductTid == shopProduct.Id);
            int quantity = buyProduct.Quantity;
            int order = buyProduct.Order;
            if (shopProduct.GoodsType == RewardType.Item || shopProduct.GoodsType.ToString().StartsWith("Equipment"))
            {
                AddItemById(user, ref response, itemId: shopProduct.GoodsId, RewardType.Item, shopProduct.GoodsValue, quantity, order);
            }
            else if (shopProduct.GoodsType == RewardType.Currency)
            {
                long val = shopProduct.GoodsValue * quantity;
                user.AddCurrency((CurrencyType)shopProduct.GoodsId, val);
                // buyCounts.Add(new() { Order = order, BuyCount = quantity });
                response.Product.Currency.Add(new NetCurrencyData() { Type = shopProduct.GoodsId, Value = val, FinalValue = user.GetCurrencyVal((CurrencyType)shopProduct.GoodsId) });
            }
            else if (shopProduct.GoodsType == RewardType.Character)
            {
                AddCharacterByCharacterTid(user, ref response, shopProduct.GoodsId, shopProduct.GoodsValue, quantity, order);
            }
            else if (shopProduct.GoodsType == RewardType.UserTitle)
            {
                response.Product.UserTitleList.Add(shopProduct.GoodsId);
            }
            else if (shopProduct.GoodsType == RewardType.LiveWallpaper)
            {
                response.Product.LiveWallPapers.Add(shopProduct.GoodsId);
            }
            else
            {
                Logging.WriteLine($"Unsupported GoodsType: {shopProduct.GoodsType}");
            }
        }

        JsonDb.Save();

        return true;
    }

    private static void DeductUserCurrencyAndItems(User user, Dictionary<int, int> totalCurrencyPrice, Dictionary<int, int> totalItemPrice,
        ref ResEventShopMultipleBuyProduct response)
    {
        foreach (int key in totalCurrencyPrice.Keys)
        {
            CurrencyType currencyType = (CurrencyType)key;
            user.SubtractCurrency(currencyType, totalCurrencyPrice[key]);
            response.Currencies.Add(new NetUserCurrencyData() { Type = key, Value = user.GetCurrencyVal(currencyType) });
            if (currencyType == CurrencyType.FreeCash)
            {
                response.Currencies.Add(new NetUserCurrencyData() { Type = (int)CurrencyType.ChargeCash, Value = user.GetCurrencyVal(CurrencyType.ChargeCash) });
            }
        }
        foreach (int tid in totalItemPrice.Keys)
        {
            var item = user.Items.FirstOrDefault(i => i.ItemType == tid);
            if (item != null)
            {
                user.RemoveItemBySerialNumber(item.Isn, totalItemPrice[tid]);
                response.Items.Add(new NetUserItemData() { Tid = tid, Count = user.Items.FirstOrDefault(i => i.ItemType == tid)?.Count ?? 0, Isn = item.Isn });
            }
        }
    }

    private static bool CheckUserCurrencyAndItemBalance(User user, List<ContentsShopProductRecord> shopProducts, List<NetBuyProductRequestData> buyProducts,
        out Dictionary<int, int> totalCurrencyPrices, out Dictionary<int, int> totalItemPrices)
    {
        totalCurrencyPrices = shopProducts
            .Where(sp => sp.PriceType == PriceType.Currency)
            .GroupBy(sp => sp.PriceId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(sp => sp.PriceValue * buyProducts.FirstOrDefault(bp => bp.ShopProductTid == sp.Id).Quantity)
            );

        totalItemPrices = shopProducts
            .Where(sp => sp.PriceType == PriceType.Item)
            .GroupBy(sp => sp.PriceId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(sp => sp.PriceValue * buyProducts.FirstOrDefault(bp => bp.ShopProductTid == sp.Id).Quantity)
            );

        Logging.WriteLine($"totalCurrencyPrice: {JsonConvert.SerializeObject(totalCurrencyPrices)}", LogType.Debug);
        foreach (int currencyType in totalCurrencyPrices.Keys)
        {
            if (!user.CanSubtractCurrency((CurrencyType)currencyType, totalCurrencyPrices[currencyType]))
            {
                long userCurrency = (CurrencyType)currencyType == CurrencyType.FreeCash
                    ? user.GetCurrencyVal(CurrencyType.FreeCash) + user.GetCurrencyVal(CurrencyType.ChargeCash)
                    : user.GetCurrencyVal((CurrencyType)currencyType);
                Logging.WriteLine($"Insufficient funds: Have {userCurrency}, need {totalCurrencyPrices[currencyType]}");
                return false;
            }
        }

        Logging.WriteLine($"totalItemPrice: {JsonConvert.SerializeObject(totalItemPrices)}", LogType.Debug);
        foreach (int tid in totalItemPrices.Keys)
        {
            var item = user.Items.FirstOrDefault(i => i.ItemType == tid);
            if (item == null || item.Count < totalItemPrices[tid])
            {
                Logging.WriteLine($"Insufficient item funds: Have {item?.Count ?? 0}, need {totalItemPrices[tid]}");
                return false;
            }
        }
        return true;
    }


    public static int GetEventShopId(int eventId)
    {
        if (eventId <= 0) return 0;

        var contentsShopTable = GameData.Instance.ContentsShopTable;

        if (GameData.Instance.eventManagers.TryGetValue(eventId, out var eventRecord))
        {
            // 1. Direct check: if eventRecord.EventShortcutId is a valid ContentsShopTable ID
            if (int.TryParse(eventRecord.EventShortcutId, out int shortcutId) && contentsShopTable.ContainsKey(shortcutId))
            {
                return shortcutId;
            }

            // 2. Child ShopEvent check (common for FieldHubEvents like 80900 Neverland, 81400 Beauty Full Shot, 81200 Last Kingdom)
            var childShopEvent = GameData.Instance.eventManagers.Values.FirstOrDefault(e =>
                (e.ParentsEventId == eventId || e.SetField == eventId) && e.EventSystemType == EventSystemType.ShopEvent);
            if (childShopEvent != null && int.TryParse(childShopEvent.EventShortcutId, out int childShortcutId) && contentsShopTable.ContainsKey(childShortcutId))
            {
                return childShortcutId;
            }

            // 3. Parent event's shop check (if this is a child event whose parent has a ShopEvent)
            if (eventRecord.ParentsEventId > 0)
            {
                var parentShopEvent = GameData.Instance.eventManagers.Values.FirstOrDefault(e =>
                    (e.ParentsEventId == eventRecord.ParentsEventId || e.SetField == eventRecord.ParentsEventId) && e.EventSystemType == EventSystemType.ShopEvent);
                if (parentShopEvent != null && int.TryParse(parentShopEvent.EventShortcutId, out int parentShortcutId) && contentsShopTable.ContainsKey(parentShortcutId))
                {
                    return parentShortcutId;
                }
            }
        }

        // 4. Standard formula: 9000000 + (eventId % 1000) * 100 + 1 (e.g. 40120 -> 9012001, 40105 -> 9010501)
        int formulaShopId = 9000000 + (eventId % 1000) * 100 + 1;
        if (contentsShopTable.ContainsKey(formulaShopId))
        {
            return formulaShopId;
        }

        // 5. Bundle lookup: BundleId == 90000 + (eventId % 1000)
        int bundleId = 90000 + (eventId % 1000);
        var shopByBundle = contentsShopTable.Values.FirstOrDefault(s => s.BundleId == bundleId);
        if (shopByBundle != null)
        {
            return shopByBundle.Id;
        }

        log.Warn($"Event shop not found for EventId: {eventId}");
        return 0;
    }

    /// <summary>
    /// Initialize Shop Data
    /// </summary>
    /// <param name="shopId">Shop ID</param>
    /// <returns>Shop Data</returns>
    public static NetEventShopProductData InitShopData(User user, int eventId)
    {
        NetEventShopProductData shop = new();

        var shopId = GetEventShopId(eventId);
        if (shopId <= 0) return shop;

        try
        {
            if (!GameData.Instance.ContentsShopTable.TryGetValue(shopId, out var tableShop))
            {
                Logging.WriteLine($"Invalid shopId: {shopId}", LogType.Warning);
                return shop;
            }

            var userBuyCounts = new List<EventShopProductData>();
            if (user.EventShopBuyCountInfo.TryGetValue(eventId, out var userBuyCountInfo)) userBuyCounts = userBuyCountInfo.datas;

            shop.ShopTid = tableShop.Id;
            shop.ShopCategory = (int)tableShop.ShopCategory;
            GameData.Instance.ContentsShopProductTable.Values
                .Where(csp => csp.BundleId == tableShop.BundleId).ToList().ForEach(csp =>
                {
                    var buyCount = userBuyCounts.FirstOrDefault(x => x.ProductTid == csp.Id)?.BuyCount ?? 0;
                    shop.List.Add(new NetShopProductInfoData
                    {
                        Order = csp.ProductOrder,
                        ProductId = csp.Id,
                        BuyLimitCount = csp.BuyLimitCount,
                        BuyCount = buyCount,
                        // Discount = csp.DiscountProbId,
                    });
                });

            return shop;
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"Error in InitShopData: {ex}");
            return shop;
        }
    }

    public static List<string> GetShopIds()
    {
        List<string> shopIds = [];
        var ContentsShopTable = GameData.Instance.ContentsShopTable;
        var eventShops = ContentsShopTable.Values.Where(s => s.ShopCategory == ShopCategoryType.ShopStoryEvent || s.ShopType == ShopType.EventShop).ToList();
        foreach (var shop in eventShops)
        {
            shopIds.Add(shop.Id.ToString());
        }
        log.Debug($"Final list of shop IDs: {JsonConvert.SerializeObject(shopIds)}");
        return shopIds;
    }

    public static bool UpdateCurrency(User user, int priceId, int priceValue, int quantity, ref ResEventShopBuyProduct response)
    {
        long totalPrice = (long)priceValue * quantity;
        CurrencyType currencyType = (CurrencyType)priceId;
        if (!user.CanSubtractCurrency(currencyType, totalPrice))
        {
            long currentAmount = currencyType == CurrencyType.FreeCash
                ? user.GetCurrencyVal(CurrencyType.FreeCash) + user.GetCurrencyVal(CurrencyType.ChargeCash)
                : user.GetCurrencyVal(currencyType);
            Logging.WriteLine($"Insufficient funds: Have {currentAmount}, need {totalPrice}");
            return false;
        }

        user.SubtractCurrency(currencyType, totalPrice);
        response.Currencies.Add(new NetUserCurrencyData
        {
            Type = (int)currencyType,
            Value = user.GetCurrencyVal(currencyType)
        });
        if (currencyType == CurrencyType.FreeCash)
        {
            response.Currencies.Add(new NetUserCurrencyData
            {
                Type = (int)CurrencyType.ChargeCash,
                Value = user.GetCurrencyVal(CurrencyType.ChargeCash)
            });
        }
        return true;
    }

    public static bool UpdateItem(User user, int priceId, int priceValue, int quantity, ref ResEventShopBuyProduct response)
    {
        var item = user.Items.FirstOrDefault(i => i.ItemType == priceId);
        if (item == null || item.Count < quantity)
        {
            Logging.WriteLine($"Insufficient item funds: Have {item?.Count ?? 0}, need {priceValue * quantity}");
            return false; // Not enough items
        }
        else
        {
            item.Count -= priceValue * quantity; // Deduct the item cost
            if (item.Count <= 0)
            {
                user.Items.Remove(item); // Remove item if count is zero or less
            }
            // Update response items
            response.Item = new()
            {
                Tid = item.ItemType,
                Count = item.Count,
                Isn = item.Isn
            };
        }
        return true;
    }

    public static void AddCharacterByCharacterTid(User user, ref ResEventShopMultipleBuyProduct response, int characterTid, int goodsValue, int quantity, int order)
    {
        // Get character data from GameData.Instance.CharacterTable
        if (!GameData.Instance.CharacterTable.TryGetValue(characterTid, out var characterRecord))
        {
            return; // Character data not found, return
        }
        // Check if character already exists in user.Characters
        var userCharacter = user.GetCharacter(characterTid);
        bool isAddNewCharacter = userCharacter == null;
        if (isAddNewCharacter)
        {
            // Add new character to user.Characters
            userCharacter = new CharacterModel()
            {
                Csn = user.GenerateUniqueCharacterId(),
                Grade = 1,
                Tid = characterRecord.Id,
            };
            user.Characters.Add(userCharacter);
            response.Product.UserCharacters.Add(ToNetUserCharacter(userCharacter));
        }
        NetCharacterData netCharacter = new() { Csn = userCharacter.Csn, Tid = userCharacter.Tid };

        // Calculate character material num
        int characterMaterialNum = isAddNewCharacter ? goodsValue * quantity - 1 : goodsValue * quantity;
        if (characterMaterialNum > 0)
        {
            var currentOriginalRare = characterRecord.OriginalRare;
            // Get max core num
            int maxCoreNum = currentOriginalRare == OriginalRareType.SSR ? 11 : currentOriginalRare == OriginalRareType.SR ? 3 : 1;

            // Get current core num
            int currentCoreNum = currentOriginalRare == OriginalRareType.SSR ? userCharacter.Grade : currentOriginalRare == OriginalRareType.SR ? userCharacter.Grade % 100 : 1;
            // If current core num is greater than max core num, set current core num to max core num
            if (currentCoreNum > maxCoreNum) currentCoreNum = maxCoreNum;
            int currentMaterialNum = user.Items.FirstOrDefault(x => x.ItemType == characterRecord.PieceId)?.Count ?? 0;
            bool isAddMaterial = currentCoreNum < maxCoreNum;
            int addMaterialNum = characterMaterialNum - currentMaterialNum;
            int addCurrencyNum = 0;
            bool isAddCurrency = currentCoreNum + addMaterialNum > maxCoreNum;
            if (isAddCurrency)
            {
                int MaterialCurrencyNum = currentOriginalRare == OriginalRareType.SSR ? 6000 : currentOriginalRare == OriginalRareType.SR ? 200 : 150;
                addCurrencyNum = (currentCoreNum + addMaterialNum - maxCoreNum) * MaterialCurrencyNum;
                addMaterialNum = maxCoreNum - currentCoreNum;
            }
            Dictionary<CurrencyType, long> currency = [];
            if (addCurrencyNum > 0)
            {
                netCharacter.CurrencyValue = addCurrencyNum;
                user.AddCurrency(CurrencyType.DissolutionPoint, addCurrencyNum);
                currency.Add(CurrencyType.DissolutionPoint, addCurrencyNum);
            }
            List<DbItemData> items = [];
            List<DbItemData> userItems = [];
            if (addMaterialNum > 0)
            {
                netCharacter.PieceCount = addMaterialNum;
                AddItemById(user, ref response, characterRecord.PieceId, RewardType.Item, goodsValue, addMaterialNum, order);
            }
            response.Product.Character.Add(netCharacter);
        }

    }

    public static void AddItemById(User user, ref ResEventShopMultipleBuyProduct response,
        int itemId, RewardType itemType, int goodsValue, int quantity, int order)
    {
        var userItemIndex = user.Items.FindIndex(i => i.ItemType == itemId);
        var isEquip = GameData.Instance.ItemEquipTable.TryGetValue(itemId, out var equip);
        if (userItemIndex >= 0)
        {
            if (isEquip)
            {
                // the item is not stackable, we need to create new entries for each quantity
                for (int i = 0; i < goodsValue * quantity; i++)
                {
                    var (tid, pos, isn) = (itemId, GetItemPos(equip.ItemSubType), user.GenerateUniqueItemId());
                    DbItemData newItem = new() { ItemType = tid, Count = 1, Position = pos, Isn = isn, Corp = GetEquipCorp(itemType) };
                    user.Items.Add(newItem);
                    response.Product.Item.Add(NetUtils.ItemDataToNet(newItem));
                    response.Product.UserItems.Add(NetUtils.UserItemDataToNet(newItem));
                }
            }
            else
            {
                user.Items[userItemIndex].Count += goodsValue * quantity;
                response.Product.UserItems.Add(NetUtils.UserItemDataToNet(user.Items[userItemIndex]));
                var (tid, count, isn) = (itemId, goodsValue * quantity, user.Items[userItemIndex].Isn);
                bool isAddAutoCharge = AddAutoChargeByTid(ref response, itemId: itemId, value: count, finalValue: user.Items[userItemIndex].Count);
                if (!isAddAutoCharge)
                {
                    response.Product.Item.Add(new NetItemData() { Tid = tid, Count = count, Isn = isn });
                }
            }
        }
        else
        {
            var (tid, count, isn) = (itemId, goodsValue * quantity, user.GenerateUniqueItemId());
            DbItemData itemData = new() { ItemType = tid, Count = count, Isn = isn };
            user.Items.Add(itemData);
            response.Product.UserItems.Add(NetUtils.UserItemDataToNet(itemData));
            bool isAddAutoCharge = AddAutoChargeByTid(ref response, itemId: itemId, value: count, finalValue: count);
            if (!isAddAutoCharge)
            {
                response.Product.Item.Add(NetUtils.ItemDataToNet(itemData));
            }
        }
    }

    public static bool AddAutoChargeByTid(ref ResEventShopMultipleBuyProduct response, int itemId, int value, int finalValue)
    {
        var autoCharge = GameData.Instance.AutoChargeTable.Values.FirstOrDefault(x => x.ItemId == itemId);
        if (autoCharge is null) return false;
        response.Product.AutoCharge.Add(new NetAutoChargeData() { AutoChargeId = autoCharge.Id, Value = value, FinalValue = finalValue });
        return true;
    }

    public static int GetItemPos(ItemSubType subType)
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

    public static int GetEquipCorp(RewardType subType)
    {
        return subType switch
        {
            RewardType.EquipmentELYSION => 1,
            RewardType.EquipmentMISSILIS => 2,
            RewardType.EquipmentTETRA => 3,
            RewardType.EquipmentPILGRIM => 4,
            RewardType.EquipmentABNORMAL => 7,
            _ => 0,
        };
    }

    public static NetUserCharacterDefaultData ToNetUserCharacter(CharacterModel character)
    {
        return new()
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