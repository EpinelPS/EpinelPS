using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/allclearequipment")]
    public class ClearAllEquipment : LobbyMsgHandler
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
                    // update character Id
                    item.Csn = 0;

                    response.Items.Add(NetUtils.ToNet(item));
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
