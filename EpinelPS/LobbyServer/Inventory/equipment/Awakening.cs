using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;


namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/awakening")]
    public class AwakeningEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEquipmentAwakening req = await ReadData<ReqEquipmentAwakening>();
            User user = GetUser();

            ResEquipmentAwakening response = new();

            ItemData? equipmentToAwaken = user.Items.FirstOrDefault(x => x.Isn == req.Isn);
            if (equipmentToAwaken == null)
            {
                await WriteDataAsync(response);
                return;
            }

            int materialCost = 1;
            int materialId = 7080001; // Equipment option material

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

            int awakenedEquipmentTypeId = GetAwakenedEquipmentTypeId(equipmentToAwaken.ItemType);
            
            equipmentToAwaken.ItemType = awakenedEquipmentTypeId;
            equipmentToAwaken.Level = 0;
            equipmentToAwaken.Exp = 0;
            equipmentToAwaken.Corp = 0;

            Random rng = new Random();
            
            NetEquipmentAwakeningOption awakeningOption = new NetEquipmentAwakeningOption()
            {
                Option1Lock = false,
                IsOption1DisposableLock = false,
                Option2Lock = false,
                IsOption2DisposableLock = false,
                Option3Lock = false,
                IsOption3DisposableLock = false
            };

            if (GameData.Instance.ItemEquipTable.TryGetValue(awakenedEquipmentTypeId, out ItemEquipRecord? equipRecord))
            {
                try
                {
                    GenerateAwakeningOptions(awakeningOption, equipRecord, rng);
                }
                catch (InvalidOperationException ex)
                {
                    Logging.WriteLine($"Failed to generate awakening options: {ex.Message}", LogType.Error);
                    await WriteDataAsync(response);
                    return;
                }
            }

            user.EquipmentAwakenings.Add(new EquipmentAwakeningData()
            {
                Isn = equipmentToAwaken.Isn,
                Option = awakeningOption,
                IsNewData = false
            });

            response.Awakening = new NetEquipmentAwakening()
            {
                Isn = equipmentToAwaken.Isn,
                Option = awakeningOption
            };
            
            response.Items.Add(NetUtils.ToNet(equipmentToAwaken));

            JsonDb.Save();
            await WriteDataAsync(response);
        }
        
        private int GetAwakenedEquipmentTypeId(int originalTypeId)
        {
            // Equipment ID format: 3 + Slot(1Head2Body3Arm4Leg) + Class(1Attacker2Defender3Supporter) + Rarity(01-09 T1-T9, 10 T10) + 01
            // Awakening changes T9 equipment (09) to T10 equipment (10)
            return originalTypeId switch
            {
                // Attacker equipment awakening
                3110901 => 3111001, // Head T9 -> T10
                3210901 => 3211001, // Body T9 -> T10
                3310901 => 3311001, // Arm T9 -> T10
                3410901 => 3411001, // Leg T9 -> T10
                
                // Defender equipment awakening
                3120901 => 3121001, // Head T9 -> T10
                3220901 => 3221001, // Body T9 -> T10
                3320901 => 3321001, // Arm T9 -> T10
                3420901 => 3421001, // Leg T9 -> T10
                
                // Supporter equipment awakening
                3130901 => 3131001, // Head T9 -> T10
                3230901 => 3231001, // Body T9 -> T10
                3330901 => 3331001, // Arm T9 -> T10
                3430901 => 3431001, // Leg T9 -> T10
                
                // Default return original ID (awakening not supported)
                _ => originalTypeId
            };
        }
        
        private void GenerateAwakeningOptions(NetEquipmentAwakeningOption option, ItemEquipRecord equipRecord, Random rng)
        {
            List<int> excludedStateEffectIds = new List<int>();

            // 1.0 = 100% chance for slot 1, 0.5 = 50% chance for slot 2, 0.3 = 30% chance for slot 3
            double[] slotActivationProbabilities = { 1.0, 0.5, 0.3 };

            for (int i = 1; i <= 3; i++)
            {
                bool shouldActivateSlot = rng.NextDouble() < slotActivationProbabilities[i - 1];

                if (shouldActivateSlot)
                {
                    int selectedOptionId = GenerateNewOptionIdInit(excludedStateEffectIds, 11);
                    AddOptionToExclusionList(selectedOptionId, excludedStateEffectIds);

                    switch (i)
                    {
                        case 1:
                            option.Option1Id = selectedOptionId;
                            break;
                        case 2:
                            option.Option2Id = selectedOptionId;
                            break;
                        case 3:
                            option.Option3Id = selectedOptionId;
                            break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 1:
                            option.Option1Id = 0;
                            break;
                        case 2:
                            option.Option2Id = 0;
                            break;
                        case 3:
                            option.Option3Id = 0;
                            break;
                    }
                }
            }

            option.Option1Lock = false;
            option.IsOption1DisposableLock = false;
            option.Option2Lock = false;
            option.IsOption2DisposableLock = false;
            option.Option3Lock = false;
            option.IsOption3DisposableLock = false;
        }

        private int GenerateNewOptionIdInit(List<int> excludedStateEffectIds, int level)
        {
            List<EquipmentOptionRecord> allAwakeningOptions = GameData.Instance.EquipmentOptionTable.Values
                .Where(x => x.EquipmentOptionGroupId == 100000 &&
                           x.StateEffectList?.Any(se => se.StateEffectLevel == level) == true)
                .ToList();

            HashSet<int> excludedEffectGroupIds = new HashSet<int>();

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

            if (optionsByEffectGroup.Count == 0)
            {
                throw new InvalidOperationException("No available equipment options for awakening - this indicates a data consistency issue");
            }

            double excludedProbabilitySum = CalculateExcludedProbabilitySumByEffectGroup(excludedEffectGroupIds, level);

            List<EffectGroupWithWeight> weightedEffectGroups = CalculateDynamicProbabilitiesForEffectGroups(optionsByEffectGroup, excludedProbabilitySum);

            int selectedEffectGroupId = SelectWeightedRandomEffectGroup(weightedEffectGroups);

            List<EquipmentOptionRecord> optionsInSelectedGroup = optionsByEffectGroup[selectedEffectGroupId];

            foreach (EquipmentOptionRecord option in optionsInSelectedGroup)
            {
                StateEffectList? stateEffect = option.StateEffectList?.FirstOrDefault(se => se.StateEffectLevel == level);
                if (stateEffect != null)
                {
                    return stateEffect.StateEffectId;
                }
            }

            throw new InvalidOperationException($"No state effect found with level {level} in selected effect group - this indicates a data consistency issue");
        }

        /// <summary>
        /// Calculates the sum of base probabilities for excluded effect groups according to the Overload system rules
        /// </summary>
        /// <param name="excludedEffectGroupIds">List of excluded state_effect_group_ids</param>
        /// <param name="level">The level of options to consider</param>
        /// <returns>Sum of base probabilities (as decimal percentage)</returns>
        private double CalculateExcludedProbabilitySumByEffectGroup(HashSet<int> excludedEffectGroupIds, int level)
        {
            if (excludedEffectGroupIds.Count == 0)
            {
                return 0.0;
            }

            double totalExcluded = 0.0;

            foreach (int effectGroupId in excludedEffectGroupIds)
            {
                List<EquipmentOptionRecord> options = GameData.Instance.EquipmentOptionTable.Values
                    .Where(opt => opt.StateEffectGroupId == effectGroupId &&
                                 opt.EquipmentOptionGroupId == 100000 &&
                                 opt.StateEffectList?.Any(se => se.StateEffectLevel == level) == true)
                    .ToList();

                if (options.Count > 0)
                {
                    EquipmentOptionRecord firstOption = options.First();
                    totalExcluded += firstOption.OptionGroupRatio / 100.0;
                }
            }

            return Math.Min(totalExcluded, 99.9); // Ensure we don't reach 100% to avoid division by zero
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

            if (probabilityDenominator <= 0)
            {
                probabilityDenominator = 1.0;
            }
            foreach (KeyValuePair<int, List<EquipmentOptionRecord>> kvp in optionsByEffectGroup)
            {
                int effectGroupId = kvp.Key;
                List<EquipmentOptionRecord> options = kvp.Value;

                if (options.Count > 0)
                {
                    EquipmentOptionRecord firstOption = options.First();
                    double baseProbability = (double)firstOption.OptionGroupRatio / 100.0;

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
        /// <returns>Selected effect_group_id</returns>
        private int SelectWeightedRandomEffectGroup(List<EffectGroupWithWeight> weightedEffectGroups)
        {
            Random random = new Random();
            double totalWeight = weightedEffectGroups.Sum(weg => weg.Weight);

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


        private void AddOptionToExclusionList(int optionId, List<int> excludedStateEffectIds)
        {
            if (!excludedStateEffectIds.Contains(optionId))
            {
                excludedStateEffectIds.Add(optionId);
            }
        }
    }
}