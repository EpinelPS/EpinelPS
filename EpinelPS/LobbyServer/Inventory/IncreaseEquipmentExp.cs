using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/increaseexpequipment")]
    public class IncreaseEquipmentExp : LobbyMsgHandler
    {
        readonly Dictionary<int, int> boostExpTable = new()
        {
            { 7010001, 100 },
            { 7010002, 1000 },
            { 7010003, 8000 }
        };
        
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqIncreaseExpEquip>();
            var user = GetUser();
            var response = new ResIncreaseExpEquip();
            var destItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ?? throw new NullReferenceException();;
            int goldCost = 0;
            int modules = 0;

            foreach (var srcItem in req.ItemList)
            {
                var item = user.Items.FirstOrDefault(x => x.Isn == srcItem.Isn) ?? throw new NullReferenceException();;
                item.Count -= srcItem.Count;

                (int addedExp, int addedModules) = AddExp(srcItem, destItem);
                goldCost += addedExp;
                modules += addedModules;

                response.Items.Add(NetUtils.ToNet(item));
            }

            response.Currency = new NetUserCurrencyData
            {
                Type = (int)CurrencyType.Gold,
                Value = user.GetCurrencyVal(CurrencyType.Gold) - MathUtils.Clamp(goldCost, 0, CalcTotalExp(destItem))
            };

            // TODO: need reward handling function first
            if (modules > 0)
            {
                (int t1, int t2, int t3) = CalcModules(modules);

                if (t1 > 0)
                {
                }
                if (t2 > 0)
                {
                }
                if (t3 > 0)
                {
                }
            }

            // we NEED to make sure the target item itself is in the delta list, or the UI won't update!
            response.Items.Add(NetUtils.ToNet(destItem));

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        (int exp, int modules) AddExp(NetItemData srcItem, ItemData destItem)
        {
            int[] maxLevel = [0, 0, 3, 3, 4, 4, 5, 5, 5, 5];
            var srcEquipRecord = GameData.Instance.ItemEquipTable.Values.FirstOrDefault(x => x.id == srcItem.Tid);
            var destEquipRecord = GameData.Instance.ItemEquipTable.Values.FirstOrDefault(x => x.id == destItem.ItemType) ?? throw new NullReferenceException();;
            int[] expNextTable = [.. GameData.Instance.itemEquipExpTable.Values
                    .Where(x => x.item_rare == destEquipRecord.item_rare)
                    .Select(x => x.exp)
                    .OrderBy(x => x)];
            int exp = 0;
            int modules = 0;

            if (srcEquipRecord != null)
            {
                var levelRecord = GameData.Instance.ItemEquipGradeExpTable.Values.FirstOrDefault(x => x.grade_core_id == srcEquipRecord.grade_core_id) ?? throw new NullReferenceException();;

                exp = srcItem.Count * levelRecord.exp;

                destItem.Exp += exp;
            }
            else // if the record is null, boost modules are being used
            {
                foreach (var entry in boostExpTable)
                {
                    if (entry.Key == srcItem.Tid)
                    {
                        exp = srcItem.Count * entry.Value;
                        break;
                    }
                }

                destItem.Exp += exp;
            }

            // TODO: double-check this. is this a thing?
            // destItem.Exp += GetSourceExp(srcItem);

            while (destItem.Level < maxLevel[destEquipRecord.grade_core_id - 1] && destItem.Exp >= expNextTable[destItem.Level + 1] && destItem.Level < maxLevel[destEquipRecord.grade_core_id - 1])
            {
                destItem.Exp -= expNextTable[destItem.Level + 1];
                destItem.Level++;
            }

            if (destItem.Level >= maxLevel[destEquipRecord.grade_core_id - 1])
            {
                modules = destItem.Exp; // TODO: check this. is the ratio actually 1:1?
                destItem.Exp = 0;
            }

            return (exp, modules);
        }

        private int GetSourceExp(NetItemData srcItem)
        {
            var item = GetUser().Items.FirstOrDefault(x => x.Isn == srcItem.Isn) ?? throw new NullReferenceException();
            var equipRecord = GameData.Instance.ItemEquipTable.Values.FirstOrDefault(x => x.id == item.ItemType) ?? throw new NullReferenceException();
            var levelRecord = GameData.Instance.ItemEquipGradeExpTable.Values.FirstOrDefault(x => x.grade_core_id == equipRecord.grade_core_id);
            int[] expNextTable = GameData.Instance.itemEquipExpTable.Values
                .Where(x => x.item_rare == equipRecord.item_rare)
                .Select(x => x.exp)
                .ToArray();
            int level = item.Level;
            int exp = item.Exp;

            while (level-- > 0)
            {
                exp += expNextTable[level];
            }

            return exp;
        }

        private static int CalcTotalExp(ItemData destItem)
        {
            int exp = 0;
            var equipRecord = GameData.Instance.ItemEquipTable.Values.FirstOrDefault(x => x.id == destItem.ItemType) ?? throw new NullReferenceException();
            var levelRecord = GameData.Instance.ItemEquipGradeExpTable.Values.FirstOrDefault(x => x.grade_core_id == equipRecord.grade_core_id);
            int[] expNextTable = GameData.Instance.itemEquipExpTable.Values
                .Where(x => x.item_rare == equipRecord.item_rare)
                .Select(x => x.exp)
                .ToArray();

            // skip the first level, it's unused
            for (int i = 1; i < expNextTable.Length; i++)
            {
                exp += expNextTable[i];
            }

            return exp;
        }
        
        (int t1, int t2, int t3) CalcModules(int exp)
            => (exp / boostExpTable[7010001], exp / boostExpTable[7010002], exp / boostExpTable[7010003]);
    }
}
