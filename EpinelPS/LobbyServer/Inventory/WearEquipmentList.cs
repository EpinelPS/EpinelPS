using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;
using System.Linq;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/wearequipmentlist")]
    public class WearEquipmentList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqWearEquipmentList req = await ReadData<ReqWearEquipmentList>();
            User user = GetUser();

            ResWearEquipmentList response = new();

            foreach (long item2 in req.IsnList)
            {
                int pos = NetUtils.GetItemPos(user, item2);

                // Check if the item being equipped is T10
                ItemData? itemToCheck = user.Items.FirstOrDefault(x => x.Isn == item2);
                if (itemToCheck != null && IsT10Equipment(itemToCheck.ItemType))
                {
                    // If trying to equip a T10 item, check if there's already a T10 item in that position
                    bool hasT10InPosition = user.Items.Any(x => x.Position == pos && x.Csn == req.Csn && IsT10Equipment(x.ItemType));
                    if (hasT10InPosition)
                    {
                        // Don't allow replacing T10 equipment
                        continue;
                    }
                }

                // Check if item still exists after previous operations
                itemToCheck = user.Items.FirstOrDefault(x => x.Isn == item2);
                if (itemToCheck == null)
                {
                    // Item no longer exists, skip this iteration
                    continue;
                }

                // unequip previous items
                foreach (ItemData item in user.Items.ToArray())
                {
                    if (item.Position == pos && item.Csn == req.Csn)
                    {
                        // Check if the item being unequipped is T10
                        if (IsT10Equipment(item.ItemType))
                        {
                            continue;
                        }

                        item.Csn = 0;
                        item.Position = 0;
                        response.Items.Add(NetUtils.ToNet(item));
                    }
                }

                // Find the item to equip
                ItemData? targetItem = user.Items.FirstOrDefault(x => x.Isn == item2);
                if (targetItem != null)
                {
                    // Handle case where we have multiple copies of the same item
                    ItemData? equippedItem = null;
                    if (targetItem.Count > 1)
                    {
                        // Reduce count of original item
                        targetItem.Count--;
                        response.Items.Add(NetUtils.ToNet(targetItem));

                        // Create a new item instance to equip
                        equippedItem = new ItemData
                        {
                            ItemType = targetItem.ItemType,
                            Isn = user.GenerateUniqueItemId(),
                            Level = targetItem.Level,
                            Exp = targetItem.Exp,
                            Count = 1,
                            Corp = targetItem.Corp,
                            Position = pos // Set the position for the new item
                        };
                        user.Items.Add(equippedItem);
                    }
                    else
                    {
                        // Use the existing item
                        equippedItem = targetItem;
                    }

                    // equip the item
                    equippedItem.Csn = req.Csn;
                    equippedItem.Position = pos;
                    response.Items.Add(NetUtils.ToNet(equippedItem));
                }
            }

            // Ensure all requested items are in the response
            // This helps the client track the specific items that were requested
            foreach (long requestedIsn in req.IsnList)
            {
                bool requestedItemAdded = response.Items.Any(x => x.Isn == requestedIsn);
                if (!requestedItemAdded)
                {
                    ItemData? requestedItem = user.Items.FirstOrDefault(x => x.Isn == requestedIsn);
                    if (requestedItem != null)
                    {
                        response.Items.Add(NetUtils.ToNet(requestedItem));
                    }
                    else
                    {
                        // If item not found, add it with count 0 to indicate it was processed
                        response.Items.Add(new NetUserItemData()
                        {
                            Isn = requestedIsn,
                            Count = 0
                        });
                    }
                }
            }

            // Add all other equipped items for this character to the response
            // This helps the client synchronize the full equipment state
            foreach (ItemData item in user.Items)
            {
                if (item.Csn == req.Csn && item.Csn != 0)
                {
                    // Check if this item was already added in the loop above
                    bool alreadyAdded = response.Items.Any(x => x.Isn == item.Isn);
                    if (!alreadyAdded)
                    {
                        response.Items.Add(NetUtils.ToNet(item));
                    }
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private bool IsT10Equipment(int itemTypeId)
        {
            // Equipment ID format: 3 + Slot(1Head2Body3Arm4Leg) + Class(1Attacker2Defender3Supporter) + Rarity(01-09 T1-T9, 10 T10) + 01
            // T10 equipment has rarity "10" in positions 3-4 (0-based indexing) for 7-digit IDs
            string itemTypeStr = itemTypeId.ToString();

            // Check if this is an equipment item (starts with 3) and has 7 digits
            if (itemTypeStr.Length == 7 && itemTypeStr[0] == '3')
            {
                // Extract the rarity part (positions 3-4)
                string rarityPart = itemTypeStr.Substring(3, 2);
                return rarityPart == "10";
            }

            return false;
        }
    }
}
