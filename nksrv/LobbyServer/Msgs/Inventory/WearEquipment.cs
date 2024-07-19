using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Inventory
{
    [PacketPath("/inventory/wearequipment")]
    public class WearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqWearEquipment>();
            var user = GetUser();

            var response = new ResWearEquipment();

            foreach (var item in user.Items.ToArray())
            {
                if (item.Isn == req.Isn)
                {
                    // update character id
                    item.Csn = req.Csn;
                    item.Position = NetUtils.GetItemPos(user, item.Isn);
                    break;
                }
            }


            foreach (var item in user.Items.ToArray())
            {
                if (item.Csn == req.Csn)
                {
                    response.Items.Add(NetUtils.ToNet(item));
                    break;
                }
            }
            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
