using nksrv.LobbyServer.Msgs.User;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Inventory
{
    [PacketPath("/inventory/clearequipment")]
    public class ClearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearEquipment>();
            var user = GetUser();

            var response = new ResClearEquipment();

            foreach (var item in user.Items.ToArray())
            {
                if (item.Isn == req.Isn)
                {
                    // update character id
                    item.Csn = 0;

                    response.Item = NetUtils.ToNet(item);
                }
            }

            JsonDb.Save();

            WriteData(response);
        }
    }
}
