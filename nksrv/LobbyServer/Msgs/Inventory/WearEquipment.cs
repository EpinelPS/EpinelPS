using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // TODO implement

            WriteData(response);
        }
    }
}
