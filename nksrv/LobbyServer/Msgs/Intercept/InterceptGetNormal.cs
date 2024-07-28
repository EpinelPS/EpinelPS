using nksrv.Net;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/getnormal")]
    public class GetInterceptNormalTable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetInterceptNormalTable>();

            var response = new ResGetInterceptNormalTable
            {

            };

            await WriteDataAsync(response);
        }
    }
}
