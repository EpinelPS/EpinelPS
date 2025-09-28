using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;
using static EpinelPS.Data.Trigger;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/levelupharmonycube")]
    public class LevelUpHarmonyCube : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqLevelUpHarmonyCube req = await ReadData<ReqLevelUpHarmonyCube>();
            User user = GetUser();

            ResLevelUpHarmonyCube response = new();

            ItemData? harmonyCubeItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn);
            if (harmonyCubeItem == null)
            {
                throw new BadHttpRequestException("Harmony cube not found", 404);
            }

            if (!GameData.Instance.ItemHarmonyCubeTable.TryGetValue(harmonyCubeItem.ItemType, out ItemHarmonyCubeRecord? harmonyCubeData))
            {
                throw new BadHttpRequestException("Item is not a harmony cube", 400);
            }

            List<ItemHarmonyCubeLevelRecord> levelData = GameData.Instance.ItemHarmonyCubeLevelTable.Values
                .Where(x => x.LevelEnhanceId == harmonyCubeData.LevelEnhanceId)
                .OrderBy(x => x.Level)
                .ToList();

            if (levelData.Count == 0)
            {
                throw new BadHttpRequestException("No level data found for this harmony cube", 400);
            }

            ItemHarmonyCubeLevelRecord? currentLevelData = levelData.FirstOrDefault(x => x.Level == harmonyCubeItem.Level);
            if (currentLevelData == null)
            {
                throw new BadHttpRequestException("Current level data not found", 400);
            }

            ItemHarmonyCubeLevelRecord? nextLevelData = levelData.FirstOrDefault(x => x.Level == harmonyCubeItem.Level + 1);
            if (nextLevelData == null)
            {
                throw new BadHttpRequestException("Harmony cube is already at max level", 400);
            }

            int requiredMaterialCount = nextLevelData.MaterialValue;
            int requiredMaterialId = nextLevelData.MaterialId;
            int requiredGold = nextLevelData.GoldValue;

            ItemData? materialItem = user.Items.FirstOrDefault(x => x.ItemType == requiredMaterialId && x.Count >= requiredMaterialCount);
            if (materialItem == null)
            {
                throw new BadHttpRequestException($"Not enough materials. Required: {requiredMaterialCount} of item {requiredMaterialId}", 400);
            }

            if (user.GetCurrencyVal(CurrencyType.Gold) < requiredGold)
            {
                throw new BadHttpRequestException($"Not enough gold. Required: {requiredGold}", 400);
            }

            materialItem.Count -= requiredMaterialCount;
            if (materialItem.Count <= 0)
            {
                user.Items.Remove(materialItem);
            }
            else
            {
                response.Items.Add(NetUtils.ToNet(materialItem));
            }

            user.Currency[CurrencyType.Gold] -= requiredGold;

            int originalLevel = harmonyCubeItem.Level;
            harmonyCubeItem.Level++;
            harmonyCubeItem.Exp = 0; // Reset exp for the new level

            user.AddTrigger(Trigger.HarmonyCubeLevel, harmonyCubeItem.Level, harmonyCubeItem.ItemType);

            if (harmonyCubeItem.Level >= levelData.Count)
            {
                user.AddTrigger(Trigger.HarmonyCubeLevelMax, 1, harmonyCubeItem.ItemType);
            }

            response.Items.Add(NetUtils.ToNet(harmonyCubeItem));

            response.Currency = new NetUserCurrencyData
            {
                Type = (int)CurrencyType.Gold,
                Value = user.GetCurrencyVal(CurrencyType.Gold)
            };

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
