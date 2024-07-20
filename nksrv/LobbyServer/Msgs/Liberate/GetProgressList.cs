using nksrv.Net;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Liberate
{
    [PacketPath("/liberate/getprogresslist")]
    public class GetProgressList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<GetLiberateProgressListRequest>();
            var user = GetUser();

            var response = new GetLiberateProgressListResponse();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
