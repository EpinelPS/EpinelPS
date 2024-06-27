using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Inventory
{
    [PacketPath("/inventory/get")]
    public class GetInventoryData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = ReadData<ReqGetInventoryData>();

            var response = new ResGetInventoryData();

            // TODO implement

            WriteData(response);
        }
    }
}
