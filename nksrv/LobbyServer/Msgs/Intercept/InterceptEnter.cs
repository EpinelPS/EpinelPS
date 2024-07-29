using nksrv.Net;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/enter")]
    public class EnterInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterIntercept>();

            var response = new ResEnterIntercept();

            await WriteDataAsync(response);
        }
    }
}
