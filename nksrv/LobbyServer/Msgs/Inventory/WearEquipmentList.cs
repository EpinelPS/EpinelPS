using nksrv.Database;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Inventory
{
    [PacketPath("/inventory/wearequipmentlist")]
    public class WearEquipmentList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqWearEquipmentList>();
            var user = GetUser();

            var response = new ResWearEquipmentList();

            // TODO optimize
            foreach (var item2 in req.IsnList)
            {
                var pos = NetUtils.GetItemPos(user, item2);

                // unequip previous items
                foreach (var item in user.Items.ToArray())
                {
                    if (item.Position == pos && item.Csn == req.Csn)
                    {
                        item.Csn = 0;
                        item.Position = 0;
                    }
                }

                foreach (var item in user.Items.ToArray())
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
