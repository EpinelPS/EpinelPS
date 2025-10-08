using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.Utils
{
    public class EquipmentUtils
    {
        /// <summary>
        /// Deducts materials from user's inventory and updates the response
        /// </summary>
        /// <param name="material">The material item to deduct</param>
        /// <param name="materialCost">Amount of material to deduct</param>
        /// <param name="user">The user whose inventory to update</param>
        /// <param name="responseItems">The response items list to update</param>
        /// <returns>True if deduction was successful, false otherwise</returns>
        public static bool DeductMaterials(ItemData material, int materialCost, User user, IList<NetUserItemData> responseItems)
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
}
