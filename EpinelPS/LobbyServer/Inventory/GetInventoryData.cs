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
            foreach (ItemData item in user.Items)
            {
                response.Items.Add(new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Lv = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position });
            }
            // TODO: HarmonyCubes, RunAwakeningIsnList, UserRedeems

            await WriteDataAsync(response);
        }
    }
}
