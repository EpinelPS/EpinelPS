using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;


namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/resetoption")]
    public class ResetOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningResetOption req = await ReadData<ReqAwakeningResetOption>();
            User user = GetUser();

            ResAwakeningResetOption response = new ResAwakeningResetOption();

            EquipmentAwakeningData? awakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);
            if (awakening == null)
            {
                await WriteDataAsync(response);
                return;
            }

            NetEquipmentAwakeningOption resetOption = new NetEquipmentAwakeningOption();
            Random random = new Random();

            (int optionId, bool isLocked, bool isDisposableLocked)[] slotLockInfo = new (int optionId, bool isLocked, bool isDisposableLocked)[3];
            List<int> lockedOptionStateEffectIds = new List<int>();

            int lockedOptionCount = 0;
            for (int i = 1; i <= 3; i++)
            {
                int currentOptionId = GetOptionIdForSlot(awakening.Option, i);
                bool isLocked = IsOptionLocked(awakening.Option, i);
                bool isDisposableLocked = IsOptionDisposableLocked(awakening.Option, i);

                slotLockInfo[i - 1] = (currentOptionId, isLocked, isDisposableLocked);

                // Count locked options for material cost calculation
                if (isLocked || isDisposableLocked)
                    lockedOptionCount++;

                // Collect locked options for exclusion list
                if (isLocked && currentOptionId != 0)
                {
                    AddOptionToExclusionList(currentOptionId, lockedOptionStateEffectIds);
                }
            }

            int costId = GetCostIdByLockedOptionCount(lockedOptionCount);

            (int materialId, int materialCost) = GetMaterialInfo(costId);

            // Check if user has enough materials
            ItemData? material = user.Items.FirstOrDefault(x => x.ItemType == materialId);
            if (material == null || material.Count < materialCost)
            {
                Logging.WriteLine($"Insufficient materials for reset operation. Need {materialCost} of item {materialId}, but have {material?.Count ?? 0}", LogType.Warning);
                await WriteDataAsync(response);
                return;
            }

            // Deduct materials for reset
            if (!EquipmentUtils.DeductMaterials(material, materialCost, user, response.Items))
            {
                Logging.WriteLine($"Insufficient materials for reset operation. Need {materialCost} of item {materialId}, but have {material?.Count ?? 0}", LogType.Warning);
                await WriteDataAsync(response);
                return;
            }

            // Process each option slot (1, 2, 3)
            ProcessOptionSlots(awakening, resetOption, slotLockInfo, lockedOptionStateEffectIds);

            // Create a new awakening entry with the same ISN to preserve the old data
            EquipmentAwakeningData newAwakening = new EquipmentAwakeningData()
            {
                Isn = awakening.Isn,
                Option = new NetEquipmentAwakeningOption()
                {
                    Option1Id = resetOption.Option1Id,
                    Option1Lock = resetOption.Option1Lock,
                    IsOption1DisposableLock = resetOption.IsOption1DisposableLock,
                    Option2Id = resetOption.Option2Id,
                    Option2Lock = resetOption.Option2Lock,
                    IsOption2DisposableLock = resetOption.IsOption2DisposableLock,
                    Option3Id = resetOption.Option3Id,
                    Option3Lock = resetOption.Option3Lock,
                    IsOption3DisposableLock = resetOption.IsOption3DisposableLock
                },
                IsNewData = true
            };

            user.EquipmentAwakenings.Add(newAwakening);

            // Add the reset options to the response
            response.ResetOption = new NetEquipmentAwakeningOption()
            {
                Option1Id = resetOption.Option1Id,
                Option1Lock = resetOption.Option1Lock,
                IsOption1DisposableLock = resetOption.IsOption1DisposableLock,
                Option2Id = resetOption.Option2Id,
                Option2Lock = resetOption.Option2Lock,
                IsOption2DisposableLock = resetOption.IsOption2DisposableLock,
                Option3Id = resetOption.Option3Id,
                Option3Lock = resetOption.Option3Lock,
                IsOption3DisposableLock = resetOption.IsOption3DisposableLock
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }


        private void ProcessOptionSlots(EquipmentAwakeningData awakening, NetEquipmentAwakeningOption resetOption, (int optionId, bool isLocked, bool isDisposableLocked)[] slotLockInfo, List<int> lockedOptionStateEffectIds)
        {
            Random random = new Random();
            // 1.0 = 100% chance for slot 1, 0.5 = 50% chance for slot 2, 0.3 = 30% chance for slot 3
            double[] slotActivationProbabilities = { 1.0, 0.5, 0.3 };

            for (int i = 1; i <= 3; i++)
            {
                (int currentOptionId, bool isLocked, bool isDisposableLocked) = slotLockInfo[i - 1];

                if (isLocked && !isDisposableLocked)
                {
                    SetOptionForSlot(resetOption, i, currentOptionId, true, false);
                }
                else if (isLocked && isDisposableLocked)
                {
                    SetOptionForSlot(resetOption, i, currentOptionId, false, false);

                    UnlockDisposableOption(awakening.Option, i);
                }
                else
                {
                    bool shouldActivateSlot = random.NextDouble() < slotActivationProbabilities[i - 1];

                    if (shouldActivateSlot)
                    {
                        // Generate new option using non-repeating system with dynamic probability
                        int newOptionId = GenerateNewOptionIdWithDynamicProbability(lockedOptionStateEffectIds);
                        SetOptionForSlot(resetOption, i, newOptionId, false, false);

                        // Add the new option to locked list to prevent duplicates in subsequent slots
                        if (newOptionId != 0)
                        {
                            AddOptionToExclusionList(newOptionId, lockedOptionStateEffectIds);
                        }
                    }
                    else
                    {
                        SetOptionForSlot(resetOption, i, 0, false, false);
                    }
                }
            }
        }

        private void UnlockDisposableOption(NetEquipmentAwakeningOption option, int slot)
        {
            SetOptionForSlot(option, slot, GetOptionIdForSlot(option, slot), false, false);
        }

        private int GetCostIdByLockedOptionCount(int lockedOptionCount)
        {
            if (lockedOptionCount == 0)
            {
                return 100001;
            }

            EquipmentOptionCostRecord? costRecord = GameData.Instance.EquipmentOptionCostTable.Values
                .FirstOrDefault(x => x.CostGroupId == 200 && x.CostLevel == lockedOptionCount);

            return costRecord?.CostId ?? 100001;
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


        private void AddOptionToExclusionList(int optionId, List<int> lockedOptionStateEffectIds)
        {
            // Since optionId is already a state_effect_id, we can directly add it to the exclusion list
            if (!lockedOptionStateEffectIds.Contains(optionId))
            {
                lockedOptionStateEffectIds.Add(optionId);

                // Also get the effect group ID for this state_effect_id to exclude the effect group
                EquipmentOptionRecord? optionRecord = GameData.Instance.EquipmentOptionTable.Values
                    .FirstOrDefault(opt => opt.StateEffectList != null && opt.StateEffectList.Any(se => se.StateEffectId == optionId));
            }
        }




        /// <summary>
        /// Generates a new option ID using the Overload system's non-repeating effect types and dynamic probability formula
        /// </summary>
        /// <param name="excludedStateEffectIds">List of state_effect_ids that are already taken and should be excluded</param>
        /// <returns>A new state_effect_id or 0 if none available</returns>
        private int GenerateNewOptionIdWithDynamicProbability(List<int> excludedStateEffectIds)
        {
            // Get all awakening options (equipment_option_group_id == 100000)
            List<EquipmentOptionRecord> allAwakeningOptions = GameData.Instance.EquipmentOptionTable.Values
                .Where(x => x.EquipmentOptionGroupId == 100000)
                .ToList();

            // Filter out options that have already been taken (non-repeating principle)
            HashSet<int> excludedEffectGroupIds = new HashSet<int>();

            // Get the effect group IDs for all excluded state effect IDs
            foreach (int stateEffectId in excludedStateEffectIds)
            {
                EquipmentOptionRecord? excludedOption = GameData.Instance.EquipmentOptionTable.Values
                    .FirstOrDefault(opt => opt.StateEffectList != null && opt.StateEffectList.Any(se => se.StateEffectId == stateEffectId));
                if (excludedOption != null)
                {
                    excludedEffectGroupIds.Add(excludedOption.StateEffectGroupId);
                }
            }

            List<EquipmentOptionRecord> availableOptions = allAwakeningOptions
                .Where(option => !excludedEffectGroupIds.Contains(option.StateEffectGroupId))
                .ToList();

            Dictionary<int, List<EquipmentOptionRecord>> optionsByEffectGroup = availableOptions
                .GroupBy(option => option.StateEffectGroupId)
                .ToDictionary(g => g.Key, g => g.ToList());

            double excludedProbabilitySum = CalculateExcludedProbabilitySumByEffectGroup(excludedEffectGroupIds);

            List<EffectGroupWithWeight> weightedEffectGroups = CalculateDynamicProbabilitiesForEffectGroups(optionsByEffectGroup, excludedProbabilitySum);
            int selectedEffectGroupId = SelectWeightedRandomEffectGroup(weightedEffectGroups);

            List<EquipmentOptionRecord> optionsInSelectedGroup = optionsByEffectGroup[selectedEffectGroupId];
            int selectedStateEffectId = SelectOptionFromGroup(optionsInSelectedGroup);
            return selectedStateEffectId;
        }



        /// <summary>
        /// Helper class to store effect group with its calculated weight for probability selection
        /// </summary>
        public class EffectGroupWithWeight
        {
            public int EffectGroupId { get; set; }
            public double Weight { get; set; }
            public double BaseProbability { get; set; }
            public double DynamicProbability { get; set; }
        }

        /// <summary>
        /// Calculates the sum of base probabilities for excluded effect groups according to the Overload system rules
        /// </summary>
        /// <param name="excludedEffectGroupIds">List of excluded state_effect_group_ids</param>
        /// <returns>Sum of base probabilities (as decimal percentage)</returns>
        private double CalculateExcludedProbabilitySumByEffectGroup(HashSet<int> excludedEffectGroupIds)
        {
            if (excludedEffectGroupIds.Count == 0)
            {
                return 0.0;
            }

            double totalExcluded = 0.0;

            foreach (int effectGroupId in excludedEffectGroupIds)
            {
                List<EquipmentOptionRecord> options = GameData.Instance.EquipmentOptionTable.Values
                    .Where(opt => opt.StateEffectGroupId == effectGroupId && opt.EquipmentOptionGroupId == 100000)
                    .ToList();

                if (options.Count > 0)
                {
                    EquipmentOptionRecord firstOption = options.First();
                    totalExcluded += firstOption.OptionGroupRatio / 100.0;
                }
            }

            // Cap the excluded probability sum to maintain mathematical validity
            return Math.Min(totalExcluded, 99.9); // Ensure we don't reach 100% to avoid division by zero
        }

        /// <summary>
        /// Calculates dynamic probabilities for available effect groups using the formula:
        /// Dynamic Probability = Display Probability / (100% - Sum of Excluded Probabilities)
        /// </summary>
        /// <param name="optionsByEffectGroup">Dictionary of available options grouped by effect group ID</param>
        /// <param name="excludedProbabilitySum">Sum of probabilities of excluded effects</param>
        /// <returns>List of weighted effect groups for random selection</returns>
        private List<EffectGroupWithWeight> CalculateDynamicProbabilitiesForEffectGroups(Dictionary<int, List<EquipmentOptionRecord>> optionsByEffectGroup, double excludedProbabilitySum)
        {
            List<EffectGroupWithWeight> weightedEffectGroups = new List<EffectGroupWithWeight>();

            double probabilityDenominator = 100.0 - excludedProbabilitySum;

            // Prevent division by zero or negative values (shouldn't happen due to capping in CalculateExcludedProbabilitySumByEffectGroup, but let's be safe)
            if (probabilityDenominator <= 0)
            {
                Logging.WriteLine($"Warning: probabilityDenominator is {probabilityDenominator}, using uniform distribution", LogType.Warning);
                probabilityDenominator = 1.0; // Use uniform distribution as fallback
            }

            foreach (KeyValuePair<int, List<EquipmentOptionRecord>> kvp in optionsByEffectGroup)
            {
                int effectGroupId = kvp.Key;
                List<EquipmentOptionRecord> options = kvp.Value;

                if (options.Count > 0)
                {
                    EquipmentOptionRecord firstOption = options.First();
                    double baseProbability = firstOption.OptionGroupRatio / 100.0;

                    double dynamicProbability = baseProbability / probabilityDenominator;

                    double selectionWeight = dynamicProbability * 1000000;

                    weightedEffectGroups.Add(new EffectGroupWithWeight
                    {
                        EffectGroupId = effectGroupId,
                        Weight = selectionWeight,
                        BaseProbability = baseProbability,
                        DynamicProbability = dynamicProbability
                    });
                }
            }

            return weightedEffectGroups;
        }

        /// <summary>
        /// Selects an effect group randomly based on calculated weights
        /// </summary>
        /// <param name="weightedEffectGroups">List of weighted effect groups</param>
        /// <returns>Selected effect_group_id or 0 if none available</returns>
        private int SelectWeightedRandomEffectGroup(List<EffectGroupWithWeight> weightedEffectGroups)
        {
            // Safety check to prevent data corruption
            if (weightedEffectGroups == null || weightedEffectGroups.Count == 0)
                throw new InvalidOperationException("No weighted effect groups available - this indicates a data consistency issue");

            Random random = new Random();
            double totalWeight = weightedEffectGroups.Sum(weg => weg.Weight);

            // Prevent division by zero which could cause unexpected behavior
            if (totalWeight <= 0)
                throw new InvalidOperationException("Invalid group weights - this indicates a data consistency issue");

            double randomValue = random.NextDouble() * totalWeight;

            double cumulativeWeight = 0.0;
            foreach (EffectGroupWithWeight weightedGroup in weightedEffectGroups)
            {
                cumulativeWeight += weightedGroup.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    return weightedGroup.EffectGroupId;
                }
            }

            return weightedEffectGroups.Last().EffectGroupId;
        }

        private static readonly Random _random = new Random();

        /// <summary>
        /// Selects an option from an effect group based on option_ratio weights and returns a state_effect_id
        /// </summary>
        /// <param name="options">List of options in the effect group</param>
        /// <returns>Selected state_effect_id</returns>
        private int SelectOptionFromGroup(List<EquipmentOptionRecord> options)
        {
            // Safety check to prevent data corruption
            if (options == null || options.Count == 0)
                throw new InvalidOperationException("No options available in group - this indicates a data consistency issue");

            long totalRatio = options.Sum(x => (long)x.OptionRatio);

            long randomValue = _random.NextInt64(0, totalRatio);
            long cumulativeRatio = 0;

            foreach (EquipmentOptionRecord? option in options)
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
