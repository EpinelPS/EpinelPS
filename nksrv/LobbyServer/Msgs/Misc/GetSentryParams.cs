using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/system/sentry/getparams")]
    public class GetSentryParams : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new SentryDataResponse();
            // TODO check proper response from real server
          await  WriteDataAsync(r);
        }
    }
}
