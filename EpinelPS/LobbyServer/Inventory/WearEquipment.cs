using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/wearequipment")]
    public class WearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqWearEquipment>();
            var user = GetUser();

            var response = new ResWearEquipment();

            var pos = NetUtils.GetItemPos(user, req.Isn);

            // unequip old item

            foreach (var item in user.Items.ToArray())
            {
                if (item.Csn == req.Csn && item.Position == pos)
                {
                    item.Csn = 0;
                }
            }

            foreach (var item in user.Items.ToArray())
            {
                if (item.Isn == req.Isn)
                {
                    // update character id
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
