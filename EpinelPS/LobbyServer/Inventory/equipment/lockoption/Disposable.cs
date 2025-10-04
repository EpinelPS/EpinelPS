using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/lockoption/disposable")]
    public class Disposable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningDisposableLockOption req = await ReadData<ReqAwakeningDisposableLockOption>();
            User user = GetUser();

            ResAwakeningDisposableLockOption response = new ResAwakeningDisposableLockOption();
            // Find the equipment awakening data
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

            switch (slot)
            {
                case 1:
                    if (req.IsLocked)
                    {
                        awakening.Option.Option1Lock = true;
                        awakening.Option.IsOption1DisposableLock = true;
                    }
                    else
                    {
                        awakening.Option.Option1Lock = false;
                        awakening.Option.IsOption1DisposableLock = false;
                    }
                    break;
                case 2:
                    if (req.IsLocked)
                    {
                        awakening.Option.Option2Lock = true;
                        awakening.Option.IsOption2DisposableLock = true;
                    }
                    else
                    {
                        awakening.Option.Option2Lock = false;
                        awakening.Option.IsOption2DisposableLock = false;
                    }
                    break;
                case 3:
                    if (req.IsLocked)
                    {
                        awakening.Option.Option3Lock = true;
                        awakening.Option.IsOption3DisposableLock = true;
                    }
                    else
                    {
                        awakening.Option.Option3Lock = false;
                        awakening.Option.IsOption3DisposableLock = false;
                    }
                    break;
            }

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

            return GetDisposableFixCostIdByLevel(lockedOptionCount,disposableLockOptionCount);
        }


        private static int GetDisposableFixCostIdByLevel(int lockedOptionCount,int disposableLockOptionCount)
        {
            EquipmentOptionCostRecord? costRecord = GameData.Instance.EquipmentOptionCostTable.Values
                .FirstOrDefault(x => x.CostGroupId == 100 && x.CostLevel == lockedOptionCount && x.DisposableFixCostLevel == disposableLockOptionCount);

            return costRecord?.DisposableFixCostId ?? 101004;
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

            return (7080002, 20); // Default material ID and cost
        }
    }
}

