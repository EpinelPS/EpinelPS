using nksrv.Net;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/get")]
    public class GetInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetInterceptData>();

            var response = new ResGetInterceptData();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}
