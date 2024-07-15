using nksrv.Net;
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
            // TODO: figure out a way to disable sentry so that Shift Up devs wouldn't be annoyed by this server

            await WriteDataAsync(r);
        }
    }
}
