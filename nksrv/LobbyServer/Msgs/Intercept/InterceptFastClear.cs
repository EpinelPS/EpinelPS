using nksrv.Net;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace nksrv.LobbyServer.Msgs.Intercept
{
    [PacketPath("/intercept/fastclear")]
    public class FastClearInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearIntercept>();

			//add some rewards here ig
			
            await WriteDataAsync(response);
        }
    }
}
