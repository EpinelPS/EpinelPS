using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/dispatch/get")]
    public class GetDispatchList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetDispatchList>();

            var response = new ResGetDispatchList();
            // TODO
          await  WriteDataAsync(response);
        }
    }
}
