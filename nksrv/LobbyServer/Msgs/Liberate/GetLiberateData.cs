using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Liberate
{
    [PacketPath("/liberate/get")]
    public class GetLiberateData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetLiberateData>();
            var user = GetUser();

            var response = new ResGetLiberateData() { };

            // TODO

            await WriteDataAsync(response);
        }
    }
}
