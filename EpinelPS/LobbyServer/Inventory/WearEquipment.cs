using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/wearequipment")]
    public class WearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqWearEquipment req = await ReadData<ReqWearEquipment>();
            User user = GetUser();

            ResWearEquipment response = new();

            int pos = NetUtils.GetItemPos(user, req.Isn);

            // unequip old item

            foreach (ItemData item in user.Items.ToArray())
            {
                if (item.Csn == req.Csn && item.Position == pos)
                {
                    item.Csn = 0;
                }
            }

            foreach (ItemData item in user.Items.ToArray())
            {
                if (item.Isn == req.Isn)
                {
                    // update character Id
                    item.Csn = req.Csn;
                    item.Position = pos;
                    response.Items.Add(NetUtils.ToNet(item));
                    break;
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
