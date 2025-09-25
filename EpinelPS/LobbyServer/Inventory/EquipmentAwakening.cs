using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/awakening")]
    public class EquipmentAwakening : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEquipmentAwakening req = await ReadData<ReqEquipmentAwakening>();
            User user = GetUser();

            ResEquipmentAwakening response = new();

            ItemData item = user.Items.FirstOrDefault(x => x.Isn == req.Isn) ?? throw new Exception($"not Isn = {req.Isn}");
            Dictionary<int, ItemEquipRecord> itemEquipDic = GameData.Instance.ItemEquipTable;
            if (itemEquipDic.TryGetValue(item.ItemType, out ItemEquipRecord? itemEquip))
            {
                foreach (var equip in itemEquipDic)
                {
                    if (equip.Value.item_rare.Equals("T10")
                    && equip.Value.item_sub_type.Equals(itemEquip.item_sub_type)
                    && equip.Value.@class.Equals(itemEquip.@class))
                    {
                        item.ItemType = equip.Key;
                        user.Items.Add(item);
                        response.Items.Add(NetUtils.ToNet(item));
                        response.Currencies.Add(new NetUserCurrencyData()
                        {
                            Type = (int)CurrencyType.Gold,
                            Value = user.Currency[CurrencyType.Gold]
                        });
                        NetEquipmentAwakening awakening = new()
                        {
                            Isn = item.Isn,
                            Option = new NetEquipmentAwakeningOption()
                            {
                                Option1Id = 7000515
                            }
                        };
                        response.Awakening = awakening;
                        if (!user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out _))
                        {
                            user.EquipmentAwakeningOptions.Add(req.Isn, new AwakeningOption()
                            {
                                Option1Id = 7000515
                            });
                        }
                    }
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
