using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Pass
{
    [PacketPath("/pass/getactive")]
    public class GetActivePassData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetActivePassData>();

            var response = new ResGetActivePassData();

            // TODO: Support events

            WriteData(response);
        }
    }
}
