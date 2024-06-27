using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Tower
{
    [PacketPath("/tower/gettowerdata")]
    public class GetTowerData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetTowerData>();

            var response = new ResGetTowerData();
            // TODO
            WriteData(response);
        }
    }
}
