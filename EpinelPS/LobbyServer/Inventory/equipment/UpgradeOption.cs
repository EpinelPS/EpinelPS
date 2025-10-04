using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/upgradeoption")]
    public class UpgradeOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningUpgradeOption req = await ReadData<ReqAwakeningUpgradeOption>();
            User user = GetUser();

            ResAwakeningUpgradeOption response = new ResAwakeningUpgradeOption();

            // Validate input parameters
            if (req.Isn <= 0)
            {
                Logging.WriteLine($"Invalid ISN: {req.Isn}", LogType.Warning);
                await WriteDataAsync(response);
                return;
            }

            // Find the equipment awakening data (prefer old data over new data)
            EquipmentAwakeningData? awakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn && !x.IsNewData);
            if (awakening == null)
            {
                await WriteDataAsync(response);
                return;
            }

            NetEquipmentAwakeningOption newOption = new NetEquipmentAwakeningOption();

            (int optionId, bool isLocked, bool isDisposableLocked)[] slotLockInfo = new (int optionId, bool isLocked, bool isDisposableLocked)[3];

            int lockedOptionCount = 0;
            for (int i = 1; i <= 3; i++)
            {
                int currentOptionId = GetOptionIdForSlot(awakening.Option, i);
                bool isLocked = IsOptionLocked(awakening.Option, i);
                bool isDisposableLocked = IsOptionDisposableLocked(awakening.Option, i);

                slotLockInfo[i - 1] = (currentOptionId, isLocked, isDisposableLocked);

                if (isLocked || isDisposableLocked)
                    lockedOptionCount++;
            }

            // Get cost ID for upgrade based on locked option count
            int costId = GetUpgradeCostId(lockedOptionCount);

            // Query actual material ID and cost from CostTable.json
            (int materialId, int materialCost)  = GetMaterialInfo(costId);

            ItemData? material = user.Items.FirstOrDefault(x => x.ItemType == materialId);
            if (material == null || material.Count < materialCost)
            {
                await WriteDataAsync(response);
                return;
            }

            if (!EquipmentUtils.DeductMaterials(material, materialCost, user, response.Items))
            {
                await WriteDataAsync(response);
                return;
            }

            // Process each option slot (1, 2, 3)
            ProcessOptionSlots(awakening, newOption, slotLockInfo);

            // Create a new awakening entry with the same ISN to preserve the old data
            EquipmentAwakeningData newAwakening = new EquipmentAwakeningData()
            {
                Isn = awakening.Isn,
                Option = new NetEquipmentAwakeningOption()
                {
                    Option1Id = newOption.Option1Id,
                    Option1Lock = newOption.Option1Lock,
                    IsOption1DisposableLock = newOption.IsOption1DisposableLock,
                    Option2Id = newOption.Option2Id,
                    Option2Lock = newOption.Option2Lock,
                    IsOption2DisposableLock = newOption.IsOption2DisposableLock,
                    Option3Id = newOption.Option3Id,
                    Option3Lock = newOption.Option3Lock,
                    IsOption3DisposableLock = newOption.IsOption3DisposableLock
                },
                IsNewData = true // newAwakening
            };

            user.EquipmentAwakenings.Add(newAwakening);

            response.ResetOption = new NetEquipmentAwakeningOption()
            {
                Option1Id = newOption.Option1Id,
                Option1Lock = newOption.Option1Lock,
                IsOption1DisposableLock = newOption.IsOption1DisposableLock,
                Option2Id = newOption.Option2Id,
                Option2Lock = newOption.Option2Lock,
                IsOption2DisposableLock = newOption.IsOption2DisposableLock,
                Option3Id = newOption.Option3Id,
                Option3Lock = newOption.Option3Lock,
                IsOption3DisposableLock = newOption.IsOption3DisposableLock
            };

            JsonDb.Save();
            await WriteDataAsync(response);
        }


        private void ProcessOptionSlots(EquipmentAwakeningData awakening, NetEquipmentAwakeningOption newOption, (int optionId, bool isLocked, bool isDisposableLocked)[] slotLockInfo)
        {
            for (int i = 1; i <= 3; i++)
            {
                (int currentOptionId, bool isLocked, bool isDisposableLocked) = slotLockInfo[i - 1];

                if (isLocked && !isDisposableLocked)
                {
                    SetOptionForSlot(newOption, i, currentOptionId, isLocked, isDisposableLocked);
                    continue;
                }

                if (isDisposableLocked)
                {
                    SetOptionForSlot(newOption, i, currentOptionId, false, false);

                    UnlockDisposableOption(awakening.Option, i);
                    continue;
                }

                if (currentOptionId == 0)
                {
                    SetOptionForSlot(newOption, i, 0, false, false);
                    continue;
                }

                int newOptionId = GenerateNewOptionId(currentOptionId);
                SetOptionForSlot(newOption, i, newOptionId, false, false);
            }
        }

        private void UnlockDisposableOption(NetEquipmentAwakeningOption option, int slot)
        {
            SetOptionForSlot(option, slot, GetOptionIdForSlot(option, slot), false, false);
        }

        private int GetOptionIdForSlot(NetEquipmentAwakeningOption option, int slot)
        {
            return slot switch
            {
                1 => option.Option1Id,
                2 => option.Option2Id,
                3 => option.Option3Id,
                _ => 0
            };
        }

        private bool IsOptionLocked(NetEquipmentAwakeningOption option, int slot)
        {
            return slot switch
            {
                1 => option.Option1Lock,
                2 => option.Option2Lock,
                3 => option.Option3Lock,
                _ => false
            };
        }

        private bool IsOptionDisposableLocked(NetEquipmentAwakeningOption option, int slot)
        {
            return slot switch
            {
                1 => option.IsOption1DisposableLock,
                2 => option.IsOption2DisposableLock,
                3 => option.IsOption3DisposableLock,
                _ => false
            };
        }

        private void SetOptionForSlot(NetEquipmentAwakeningOption option, int slot, int optionId, bool locked, bool disposableLocked)
        {
            switch (slot)
            {
                case 1:
                    option.Option1Id = optionId;
                    option.Option1Lock = locked;
                    option.IsOption1DisposableLock = disposableLocked;
                    break;
                case 2:
                    option.Option2Id = optionId;
                    option.Option2Lock = locked;
                    option.IsOption2DisposableLock = disposableLocked;
                    break;
                case 3:
                    option.Option3Id = optionId;
                    option.Option3Lock = locked;
                    option.IsOption3DisposableLock = disposableLocked;
                    break;
            }
        }

        private int GenerateNewOptionId(int currentStateEffectId)
        {
            EquipmentOptionRecord? currentOption = GameData.Instance.EquipmentOptionTable.Values
                .FirstOrDefault(option => option.StateEffectList != null && option.StateEffectList.Any(se => se.StateEffectId == currentStateEffectId));

            if (currentOption == null|| currentOption.EquipmentOptionGroupId != 100000)
            {
                throw new InvalidOperationException($"Current state_effect_id {currentStateEffectId} not found in any EquipmentOption");
            }


            int stateEffectGroupId = currentOption.StateEffectGroupId;

            List<EquipmentOptionRecord> optionsInGroup = GameData.Instance.EquipmentOptionTable.Values
                .Where(x => x.EquipmentOptionGroupId == 100000 && x.StateEffectGroupId == stateEffectGroupId)
                .ToList();

            if (optionsInGroup.Count == 0)
            {
                throw new InvalidOperationException($"No awakening options found with state_effect_group_id {stateEffectGroupId}");
            }

            return SelectOptionFromGroup(optionsInGroup);
        }


        private static readonly Random _random = new Random();

        private int SelectOptionFromGroup(List<EquipmentOptionRecord> options)
        {
            if (options == null || options.Count == 0)
                throw new InvalidOperationException("No options available in group - this indicates a data consistency issue");

            long totalRatio = options.Sum(x => (long)x.OptionRatio);

            long randomValue = _random.NextInt64(0, totalRatio);
            long cumulativeRatio = 0;

            foreach (EquipmentOptionRecord option in options)
            {
                cumulativeRatio += option.OptionRatio;
                if (randomValue < cumulativeRatio)
                {
                    // Randomly select from the StateEffectList
                    if (option.StateEffectList == null || option.StateEffectList.Count == 0)
                    {
                        throw new InvalidOperationException($"StateEffectList is null or empty for option {option.Id}");
                    }
                    int randomIndex = _random.Next(option.StateEffectList.Count);
                    return option.StateEffectList[randomIndex].StateEffectId;
                }
            }

            // Fallback: randomly select from the StateEffectList of the last option
            EquipmentOptionRecord? lastOption = options.Last();
            if (lastOption?.StateEffectList == null || lastOption.StateEffectList.Count == 0)
            {
                throw new InvalidOperationException($"StateEffectList is null or empty for fallback option {lastOption?.Id}");
            }
            int fallbackIndex = _random.Next(lastOption.StateEffectList.Count);
            return lastOption.StateEffectList[fallbackIndex].StateEffectId;
        }

        private int GetUpgradeCostId(int lockedOptionCount)
        {

            // For upgrade operation, use cost_group_id 200
            EquipmentOptionCostRecord? costRecord = GameData.Instance.EquipmentOptionCostTable.Values
                .FirstOrDefault(x => x.CostGroupId == 200 && x.CostLevel == lockedOptionCount);

            return costRecord?.CostId ?? 102001;
        }
        
        private static (int materialId, int materialCost) GetMaterialInfo(int costId)
        {
            if (GameData.Instance.costTable.TryGetValue(costId, out CostRecord? costRecord) &&
                costRecord?.Costs != null &&
                costRecord.Costs.Count > 0)
            {
                return (costRecord.Costs[0].ItemId, costRecord.Costs[0].ItemValue);
            }
            return (7080001, 1); // Default material ID and cost
        }
    }   
}
