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

            foreach (var item2 in req.IsnList)
            {
                foreach (var item in user.Items.ToArray())
                {
                    if (item2 == item.Isn)
                    {
                        item.Csn = req.Csn;
                        item.Position = NetUtils.GetItemPos(user, item.Isn);
                        response.Items.Add(NetUtils.ToNet(item));
                    }
                }
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
