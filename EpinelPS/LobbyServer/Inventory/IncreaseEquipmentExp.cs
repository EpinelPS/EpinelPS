using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/increaseexpequipment")]
    public class IncreaseEquipmentExp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqIncreaseExpEquip>();
            var user = GetUser();
            var response = new ResIncreaseExpEquip();
            var destItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn);
            int goldCost = 0;

            foreach (var srcItem in req.ItemList)
            {
                var item = user.Items.FirstOrDefault(x => x.Isn == srcItem.Isn);
                item.Count -= srcItem.Count;

                goldCost += AddExp(srcItem, destItem);

                response.Items.Add(NetUtils.ToNet(item));
            }
            
            response.Currency = new NetUserCurrencyData
            {
                Type = (int)CurrencyType.Gold,
                Value = user.GetCurrencyVal(CurrencyType.Gold) - goldCost
            };

            // we NEED to make sure the target item itself is in the delta list, or the UI won't update!
            response.Items.Add(NetUtils.ToNet(destItem));
            
            JsonDb.Save();

            await WriteDataAsync(response);
        }

        int AddExp(NetItemData srcItem, ItemData destItem)
        {
            var srcEquipRecord = GameData.Instance.itemEquipTable.Values.FirstOrDefault(x => x.id == srcItem.Tid);
            var destEquipRecord = GameData.Instance.itemEquipTable.Values.FirstOrDefault(x => x.id == destItem.ItemType);
            var levelRecord = GameData.Instance.ItemEquipGradeExpTable.Values.FirstOrDefault(x => x.grade_core_id == srcEquipRecord.grade_core_id);
            int[] maxLevel = { 0, 0, 3, 3, 4, 4, 5, 5, 5, 5 };
            int[] expNextTable = GameData.Instance.itemEquipExpTable.Values
                .Where(x => x.item_rare == destEquipRecord.item_rare)
                .Select(x => x.exp)
                .OrderBy(x => x) // order from lowest to highest
                .ToArray();
            int exp = levelRecord.exp * srcItem.Count;

            destItem.Exp += exp * srcItem.Count;
            
            // TODO: double-check this. is this a thing?
            // destItem.Exp += GetUser().Items.FirstOrDefault(x => x.Isn == srcItem.Isn).Exp;

            while (destItem.Exp >= expNextTable[destItem.Level + 1] && destItem.Level < maxLevel[destEquipRecord.grade_core_id - 1])
            {
                destItem.Exp -= expNextTable[destItem.Level - 1];
                destItem.Level++;
            }

            if (destItem.Level >= maxLevel[destEquipRecord.grade_core_id - 1])
            {
                destItem.Exp = 0;
            }

            return exp;
        }
    }
}
