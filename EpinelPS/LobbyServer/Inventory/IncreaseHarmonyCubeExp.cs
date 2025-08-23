using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;
using static EpinelPS.Data.TriggerType;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/increaseharmonycubeexp")]
    public class IncreaseHarmonyCubeExp : LobbyMsgHandler
    {
        readonly Dictionary<int, int> harmonyCubeExpTable = new()
        {
            { 7020001, 100 },   // Basic harmony cube material
            { 7020002, 1000 },  // Intermediate harmony cube material  
            { 7020003, 8000 }   // Advanced harmony cube material
        };
        
        protected override async Task HandleAsync()
        {
            ReqIncreaseExpEquip req = await ReadData<ReqIncreaseExpEquip>();
            User user = GetUser();

       

            ResIncreaseExpEquip response = new();
            
            ItemData destItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ??
                throw new BadHttpRequestException($"Target harmony cube with ISN {req.Isn} not found", 404);

            // Verify it's a harmony cube
            if (!GameData.Instance.ItemHarmonyCubeTable.TryGetValue(destItem.ItemType, out ItemHarmonyCubeRecord? harmonyCubeData))
            {
                throw new BadHttpRequestException($"Item {destItem.ItemType} is not a harmony cube", 400);
            }

            
            int goldCost = 0;

            foreach (NetItemData? srcItem in req.ItemList)
            {
                ItemData item = user.Items.FirstOrDefault(x => x.Isn == srcItem.Isn) ??
                    throw new BadHttpRequestException($"Material item with ISN {srcItem.Isn} not found", 404);

                // Validate material count
                if (item.Count < srcItem.Count)
                {
                    throw new BadHttpRequestException($"Insufficient material count. Required: {srcItem.Count}, Available: {item.Count}", 400);
                }

                // Validate material type (must be harmony cube material or another harmony cube)
                bool isValidMaterial = harmonyCubeExpTable.ContainsKey(srcItem.Tid) ||
                                     GameData.Instance.ItemHarmonyCubeTable.ContainsKey(srcItem.Tid);

                if (!isValidMaterial)
                {
                    throw new BadHttpRequestException($"Item {srcItem.Tid} is not a valid harmony cube enhancement material", 400);
                }

                item.Count -= srcItem.Count;

                int addedExp = AddExp(srcItem, destItem, harmonyCubeData);
                goldCost += addedExp;

                response.Items.Add(NetUtils.ToNet(item));

            }

            int actualGoldCost = CalculateGoldCost(destItem, harmonyCubeData, goldCost);

            if (user.GetCurrencyVal(CurrencyType.Gold) < actualGoldCost)
            {
                throw new BadHttpRequestException($"Insufficient gold. Required: {actualGoldCost}, Available: {user.GetCurrencyVal(CurrencyType.Gold)}", 400);
            }

            user.Currency[CurrencyType.Gold] -= actualGoldCost;

            response.Currency = new NetUserCurrencyData
            {
                Type = (int)CurrencyType.Gold,
                Value = user.GetCurrencyVal(CurrencyType.Gold)
            };

            response.Items.Add(NetUtils.ToNet(destItem));

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        int AddExp(NetItemData srcItem, ItemData destItem, ItemHarmonyCubeRecord harmonyCubeData)
        {
            int exp = 0;
            int originalLevel = destItem.Level;
            int originalExp = destItem.Exp;

            // Check if it's a harmony cube material
            if (harmonyCubeExpTable.TryGetValue(srcItem.Tid, out int materialExp))
            {
                exp = srcItem.Count * materialExp;
                destItem.Exp += exp;
            }
            else
            {
                if (GameData.Instance.ItemHarmonyCubeTable.TryGetValue(srcItem.Tid, out ItemHarmonyCubeRecord? srcHarmonyCube))
                {
                    exp = CalculateHarmonyCubeExp(srcItem, srcHarmonyCube);
                    destItem.Exp += exp;
                }
                
            }

            ProcessLevelUp(destItem, harmonyCubeData);
            return exp;
        }

        private int CalculateHarmonyCubeExp(NetItemData srcItem, ItemHarmonyCubeRecord srcHarmonyCube)
        {
            // Base exp based on rarity
            int baseExp = srcHarmonyCube.item_rare switch
            {
                "SSR" => 5000,
                "SR" => 2000,
                "R" => 500,
                _ => 100
            };

            return srcItem.Count * baseExp;
        }

        private void ProcessLevelUp(ItemData destItem, ItemHarmonyCubeRecord harmonyCubeData)
        {
            // Get level data for this harmony cube
            List<ItemHarmonyCubeLevelRecord> levelData = GameData.Instance.ItemHarmonyCubeLevelTable.Values
                .Where(x => x.level_enhance_id == harmonyCubeData.level_enhance_id)
                .OrderBy(x => x.level)
                .ToList();

            if (levelData.Count == 0)
            {
                return;
            }

            // Find current level data
            ItemHarmonyCubeLevelRecord? currentLevelData = levelData.FirstOrDefault(x => x.level == destItem.Level);
            if (currentLevelData == null)
            {
                return;
            }

            int originalLevel = destItem.Level;
            User user = GetUser();
            int maxLevel = levelData.Max(x => x.level);

            // Check if we can level up
            while (destItem.Level < maxLevel)
            {
                ItemHarmonyCubeLevelRecord? nextLevelData = levelData.FirstOrDefault(x => x.level == destItem.Level + 1);
                if (nextLevelData == null)
                {
                    break;
                }

                // Check if we have enough exp for next level
                int expRequired = nextLevelData.material_value * harmonyCubeExpTable.GetValueOrDefault(nextLevelData.material_id, 100);

                if (destItem.Exp >= expRequired)
                {
                    destItem.Exp -= expRequired;
                    destItem.Level++;

                    user.AddTrigger(TriggerType.HarmonyCubeLevel, destItem.Level, destItem.ItemType);
                }
                else
                {
                    break;
                }
            }

            // Check if reached max level
            if (destItem.Level >= maxLevel)
            {
                user.AddTrigger(TriggerType.HarmonyCubeLevelMax, 1, destItem.ItemType);
            }
        }

        private int CalculateGoldCost(ItemData destItem, ItemHarmonyCubeRecord harmonyCubeData, int expAdded)
        {
            ItemHarmonyCubeLevelRecord? currentLevelData = GameData.Instance.ItemHarmonyCubeLevelTable.Values
                .FirstOrDefault(x => x.level_enhance_id == harmonyCubeData.level_enhance_id && x.level == destItem.Level);

            if (currentLevelData != null)
            {
                return Math.Min(currentLevelData.gold_value, expAdded / 10);
            }

            return Math.Min(1000, expAdded / 10); // Default gold cost
        }
    }
}
