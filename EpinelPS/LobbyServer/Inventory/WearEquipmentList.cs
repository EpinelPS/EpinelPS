using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/wearequipmentlist")]
    public class WearEquipmentList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqWearEquipmentList req = await ReadData<ReqWearEquipmentList>();
            User user = GetUser();

            ResWearEquipmentList response = new();

            // TODO optimize
            foreach (long item2 in req.IsnList)
            {
                int pos = NetUtils.GetItemPos(user, item2);

                // unequip previous items
                foreach (ItemData item in user.Items.ToArray())
                {
                    if (item.Position == pos && item.Csn == req.Csn)
                    {
                        item.Csn = 0;
                        item.Position = 0;
                    }
                }

                foreach (ItemData item in user.Items.ToArray())
                {
                    if (item2 == item.Isn)
                    {
                        item.Csn = req.Csn;
                        item.Position = pos;
                        response.Items.Add(NetUtils.ToNet(item));
                    }
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
