using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.Utils;

public class EquipmentUtils
{
    public const int CustomModuleItemId = 7080001;
    public const int CustomLockItemId = 7080002;

    /// <summary>
    /// Calculates the required Custom Module and Custom Lock costs based on lock counts and operation type.
    /// </summary>
    /// <param name="costGroupId">100 for ResetOption (Change Effects), 200 for UpgradeOption (Re-roll Values)</param>
    /// <param name="permanentLockCount">Number of lines permanently locked with Custom Modules</param>
    /// <param name="disposableLockCount">Number of lines temporarily locked with Custom Locks</param>
    /// <returns>Tuple of (moduleCost, lockCost)</returns>
    public static (int moduleCost, int lockCost) CalculateAwakeningCosts(int costGroupId, int permanentLockCount, int disposableLockCount)
    {
        int moduleCost = 1;
        int lockCost = 0;

        if (costGroupId == 100) // ResetOption (Change Effects)
        {
            if (permanentLockCount == 0 && disposableLockCount == 0)
            {
                return (1, 0); // Base cost: 1 Custom Module
            }

            EquipmentOptionCostRecord? baseRecord = GameData.Instance.EquipmentOptionCostTable.Values
                .FirstOrDefault(x => x.CostGroupId == 100 && x.CostLevel == permanentLockCount);

            if (baseRecord != null && GameData.Instance.costTable.TryGetValue(baseRecord.CostId, out CostRecord? costRecord))
            {
                moduleCost = costRecord?.Costs?.FirstOrDefault(c => c.ItemId == CustomModuleItemId)?.ItemValue ?? (permanentLockCount + 2);
            }
            else
            {
                moduleCost = permanentLockCount + 2;
            }

            for (int d = 0; d < disposableLockCount; d++)
            {
                EquipmentOptionCostRecord? dispRecord = GameData.Instance.EquipmentOptionCostTable.Values
                    .FirstOrDefault(x => x.CostGroupId == 100 && x.CostLevel == permanentLockCount && x.DisposableFixCostLevel == d);

                if (dispRecord != null && dispRecord.DisposableFixCostId != 0 &&
                    GameData.Instance.costTable.TryGetValue(dispRecord.DisposableFixCostId, out CostRecord? dispCostRecord))
                {
                    int val = dispCostRecord?.Costs?.FirstOrDefault(c => c.ItemId == CustomLockItemId)?.ItemValue ?? 0;
                    lockCost += val;
                }
            }
        }
        else if (costGroupId == 200) // UpgradeOption (Re-roll Values)
        {
            int totalLocked = permanentLockCount + disposableLockCount;

            EquipmentOptionCostRecord? costRecord = GameData.Instance.EquipmentOptionCostTable.Values
                .FirstOrDefault(x => x.CostGroupId == 200 && x.CostLevel == totalLocked);

            if (costRecord != null && GameData.Instance.costTable.TryGetValue(costRecord.CostId, out CostRecord? cr))
            {
                moduleCost = cr?.Costs?.FirstOrDefault(c => c.ItemId == CustomModuleItemId)?.ItemValue ?? (totalLocked + 1);
            }
            else
            {
                moduleCost = totalLocked + 1;
            }

            for (int d = 0; d < disposableLockCount; d++)
            {
                EquipmentOptionCostRecord? dispRecord = GameData.Instance.EquipmentOptionCostTable.Values
                    .FirstOrDefault(x => x.CostGroupId == 100 && x.CostLevel == permanentLockCount && x.DisposableFixCostLevel == d);

                if (dispRecord != null && dispRecord.DisposableFixCostId != 0 &&
                    GameData.Instance.costTable.TryGetValue(dispRecord.DisposableFixCostId, out CostRecord? dispCostRecord))
                {
                    int val = dispCostRecord?.Costs?.FirstOrDefault(c => c.ItemId == CustomLockItemId)?.ItemValue ?? 0;
                    lockCost += val;
                }
            }
        }

        return (moduleCost, lockCost);
    }

    /// <summary>
    /// Deducts Custom Modules and Custom Locks from user's inventory and updates response items.
    /// Returns true if deduction was successful, false if insufficient materials.
    /// </summary>
    public static bool DeductAwakeningMaterials(User user, int moduleCost, int lockCost, IList<NetUserItemData> responseItems)
    {
        DbItemData? moduleItem = user.Items.FirstOrDefault(x => x.ItemType == CustomModuleItemId);
        DbItemData? lockItem = user.Items.FirstOrDefault(x => x.ItemType == CustomLockItemId);

        if (moduleCost > 0 && (moduleItem == null || moduleItem.Count < moduleCost))
        {
            Logging.WriteLine($"Insufficient Custom Modules for operation. Need {moduleCost}, but have {moduleItem?.Count ?? 0}", LogType.Warning);
            return false;
        }

        if (lockCost > 0 && (lockItem == null || lockItem.Count < lockCost))
        {
            Logging.WriteLine($"Insufficient Custom Locks for operation. Need {lockCost}, but have {lockItem?.Count ?? 0}", LogType.Warning);
            return false;
        }

        if (moduleCost > 0 && moduleItem != null)
        {
            if (!DeductMaterials(moduleItem, moduleCost, user, responseItems))
                return false;
        }

        if (lockCost > 0 && lockItem != null)
        {
            if (!DeductMaterials(lockItem, lockCost, user, responseItems))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Deducts materials from user's inventory and updates the response
    /// </summary>
    /// <param name="material">The material item to deduct</param>
    /// <param name="materialCost">Amount of material to deduct</param>
    /// <param name="user">The user whose inventory to update</param>
    /// <param name="responseItems">The response items list to update</param>
    /// <returns>True if deduction was successful, false otherwise</returns>
    public static bool DeductMaterials(DbItemData material, int materialCost, User user, IList<NetUserItemData> responseItems)
    {
        if (material.Count < materialCost)
            return false;

        material.Count -= materialCost;
        if (material.Count <= 0)
        {
            user.Items.Remove(material);
            NetUserItemData netItem = NetUtils.ToNet(material);
            netItem.Count = 0;
            responseItems.Add(netItem);
        }
        else
        {
            responseItems.Add(NetUtils.ToNet(material));
        }

        return true;
    }
}

