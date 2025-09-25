using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/get")]
    public class GetInventoryData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetInventoryData req = await ReadData<ReqGetInventoryData>();
            User user = GetUser();

            ResGetInventoryData response = new();

            Dictionary<int, ItemHarmonyCubeRecord> itemHarmonyCubeDic = GameData.Instance.ItemHarmonyCubeTable;
            Dictionary<int, ItemEquipRecord> itemEquipDic = GameData.Instance.ItemEquipTable;

            response.ShowCorporationEquipmentActive = true;
            foreach (ItemData item in user.Items)
            {
                response.Items.Add(new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Lv = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position });

                // 显示已经穿戴的和谐立方
                if (itemHarmonyCubeDic.TryGetValue(item.ItemType, out _))
                {
                    NetUserHarmonyCubeData netHarmonyCube = new()
                    {
                        Isn = item.Isn,
                        Tid = item.ItemType,
                        Lv = item.Level
                    };

                    foreach (long csn in item.CsnList)
                    {
                        netHarmonyCube.CsnList.Add(csn);
                    }

                    if (item.Csn > 0 && !item.CsnList.Contains(item.Csn))
                    {
                        netHarmonyCube.CsnList.Add(item.Csn);
                    }

                    response.HarmonyCubes.Add(netHarmonyCube);
                    response.ArenaHarmonyCubes.Add(netHarmonyCube);
                }

                // 手动添加到jsondb的T10装备穿戴后有修改词条选项
                if (itemEquipDic.TryGetValue(item.ItemType, out ItemEquipRecord? itemEquip))
                {
                    if (itemEquip.item_rare.Equals("T10")
                    && !user.EquipmentAwakeningOptions.TryGetValue(item.Isn, out _))
                    {
                        response.Awakenings.Add(new NetEquipmentAwakening
                        {
                            Isn = item.Isn,
                            Option = new NetEquipmentAwakeningOption
                            {
                                Option1Id = 0
                            }
                        });
                    }
                }
            }

            
            foreach (var awakening in user.EquipmentAwakeningOptions)
            {
                response.Awakenings.Add(new NetEquipmentAwakening()
                {
                    Isn = awakening.Key,
                    Option = new NetEquipmentAwakeningOption
                    {
                        Option1Id = awakening.Value.Option1Id,
                        Option1Lock = awakening.Value.Option1Lock,
                        IsOption1DisposableLock = awakening.Value.IsOption1DisposableLock,
                        Option2Id = awakening.Value.Option2Id,
                        Option2Lock = awakening.Value.Option2Lock,
                        IsOption2DisposableLock = awakening.Value.IsOption2DisposableLock,
                        Option3Id = awakening.Value.Option3Id,
                        Option3Lock = awakening.Value.Option3Lock,
                        IsOption3DisposableLock = awakening.Value.IsOption3DisposableLock
                    }
                });
            }

            // TODO: RunAwakeningIsnList, UserRedeems

            await WriteDataAsync(response);
        }
    }
}
