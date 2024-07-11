using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Pass
{
    [PacketPath("/pass/event/getactive")]
    public class GetActiveEventPassData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetActiveEventPassData>();

            var response = new ResGetActiveEventPassData();

            // TODO: Support events

          await  WriteDataAsync(response);
        }
    }
}
