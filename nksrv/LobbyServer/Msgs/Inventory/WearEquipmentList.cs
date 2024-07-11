using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            foreach (var item in user.Items.ToArray())
            {
                foreach (var item2 in req.IsnList)
                {
                    if (item2 == item.Isn)
                    {
                        item.Csn = req.Csn;

                        response.Items.Add(NetUtils.ToNet(item));
                    }
                }
            }

            JsonDb.Save();

            WriteData(response);
        }
    }
}
