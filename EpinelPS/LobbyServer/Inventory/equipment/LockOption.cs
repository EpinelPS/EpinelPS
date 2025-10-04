using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;


namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/lockoption")]
    public class LockOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningLockOption req = await ReadData<ReqAwakeningLockOption>();
            User user = GetUser();

            ResAwakeningLockOption response = new ResAwakeningLockOption();
            EquipmentAwakeningData? awakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);
            if (awakening == null)
            {
                await WriteDataAsync(response);
                return;
            }

            int slot = 0;
            if (awakening.Option.Option1Id == req.OptionId)
                slot = 1;
            else if (awakening.Option.Option2Id == req.OptionId)
                slot = 2;
            else if (awakening.Option.Option3Id == req.OptionId)
                slot = 3;

            (int materialId, int materialCost) = GetMaterialInfoForAwakening(awakening.Option);


            ItemData? material = user.Items.FirstOrDefault(x => x.ItemType == materialId);
            if (material == null || material.Count < materialCost)
            {
                await WriteDataAsync(response);
                return;
            }

            UpdateLockStatus(awakening.Option, slot, req.IsLocked);

            if (req.IsLocked)
            {
                if (!EquipmentUtils.DeductMaterials(material, materialCost, user, response.Items))
                {
                    await WriteDataAsync(response);
                    return;
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private static int CalculateMaterialCost(NetEquipmentAwakeningOption option)
        {
            int lockedOptionCount = 0;
            int disposableLockOptionCount = 0;
            // Count already permanently locked options (not disposable locks)
            if (option.Option1Id != 0 && option.Option1Lock && !option.IsOption1DisposableLock)
                lockedOptionCount++;
            if (option.Option2Id != 0 && option.Option2Lock && !option.IsOption2DisposableLock)
                lockedOptionCount++;
            if (option.Option3Id != 0 && option.Option3Lock && !option.IsOption3DisposableLock)
                lockedOptionCount++;

            if (option.Option1Id != 0 && option.Option1Lock && option.IsOption1DisposableLock)
                disposableLockOptionCount++;
            if (option.Option2Id != 0 && option.Option2Lock && option.IsOption2DisposableLock)
                disposableLockOptionCount++;
            if (option.Option3Id != 0 && option.Option3Lock && option.IsOption3DisposableLock)
                disposableLockOptionCount++;

            return GetPermanentLockCostId(lockedOptionCount,disposableLockOptionCount);
        }

        private static int GetPermanentLockCostId(int lockedOptionCount,int disposableLockOptionCount)
        {
            // For permanent locks, use cost_group_id 100
            EquipmentOptionCostRecord? costRecord = GameData.Instance.EquipmentOptionCostTable.Values
                .FirstOrDefault(x => x.CostGroupId == 100 && x.CostLevel == lockedOptionCount && x.DisposableFixCostLevel == disposableLockOptionCount);

            int costId = costRecord?.CostId ?? 101001;

            return costId;
        }

        private static void UpdateLockStatus(NetEquipmentAwakeningOption option, int slot, bool isLocked)
        {
            switch (slot)
            {
                case 1:
                    option.Option1Lock = isLocked;
                    if (isLocked)
                    {
                        option.IsOption1DisposableLock = false;
                    }
                    break;
                case 2:
                    option.Option2Lock = isLocked;
                    if (isLocked)
                    {
                        option.IsOption2DisposableLock = false;
                    }
                    break;
                case 3:
                    option.Option3Lock = isLocked;
                    if (isLocked)
                    {
                        option.IsOption3DisposableLock = false;
                    }
                    break;
            }
        }

        private static (int materialId, int materialCost) GetMaterialInfoForAwakening(NetEquipmentAwakeningOption option)
        {
            int costId = CalculateMaterialCost(option);
            return GetMaterialInfo(costId);
        }

        private static (int materialId, int materialCost) GetMaterialInfo(int costId)
        {
            if (GameData.Instance.costTable.TryGetValue(costId, out CostRecord? costRecord) &&
                costRecord?.Costs != null &&
                costRecord.Costs.Count > 0)
            {
                return (costRecord.Costs[0].ItemId, costRecord.Costs[0].ItemValue);
            }
            return (7080001, 2); // Default material ID and cost
        }

    }
}