using EpinelPS.Utils;
using EpinelPS.Data;
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
            foreach (ItemData item in user.Items)
            {
                 
                ItemSubType itemSubType = GameData.Instance.GetItemSubType(item.ItemType);
                if (itemSubType == ItemSubType.HarmonyCube)
                {
                    NetUserHarmonyCubeData harmonyCubeData = new NetUserHarmonyCubeData()
                    {
                        Tid = item.ItemType,
                        Lv = item.Level,
                        Isn = item.Isn
                    };
                    harmonyCubeData.CsnList.AddRange(item.CsnList);
                    response.HarmonyCubes.Add(harmonyCubeData);
                }
                response.Items.Add(new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Lv = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position });
                  
            }

                    
            // Add all equipment awakenings
            foreach (EquipmentAwakeningData awakening in user.EquipmentAwakenings)
            {
                response.Awakenings.Add(new NetEquipmentAwakening()
                {
                    Isn = awakening.Isn,
                    Option = awakening.Option
                });
            }
            // TODO:  UserRedeems
            // Note: HarmonyCubes are now included in the Items list above

            await WriteDataAsync(response);
        }
    }
}
