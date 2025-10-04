using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/allclearequipment")]
    public class AllClearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAllClearEquipment req = await ReadData<ReqAllClearEquipment>();
            User user = GetUser();

            ResAllClearEquipment response = new()
            {
                Csn = req.Csn
            };

            foreach (ItemData item in user.Items.ToArray())
            {
                if (item.Csn == req.Csn)
                {
                    // Check if the item being unequipped is T10
                    if (IsT10Equipment(item.ItemType))
                    {
                        
                        response.Items.Add(NetUtils.ToNet(item));
                        continue;
                    }

                    item.Csn = 0;
                    item.Position = 0; 
                    response.Items.Add(NetUtils.ToNet(item));
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
